using System.Text;
using ExpenseManager.Api.Data;
using ExpenseManager.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// --------------------
// Services
// --------------------

// Controllers (REST API)
builder.Services.AddControllers();

// Swagger (Development only)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Custom services
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<PasswordHasherService>();
builder.Services.AddScoped<FileStorageService>();
builder.Services.AddScoped<InvoiceParser>();
builder.Services.AddScoped<OcrService>();

// --------------------
// Database connection
// --------------------

// Prefer DATABASE_URL (Render), fallback to appsettings (local)
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
var rawConnectionString = databaseUrl ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(rawConnectionString))
{
    throw new InvalidOperationException("Database connection string not configured. Set DATABASE_URL environment variable or configure DefaultConnection in appsettings.json");
}

// Convert postgres:// URL -> Npgsql connection string if needed
string connectionString;
if (rawConnectionString.StartsWith("postgres://") || rawConnectionString.StartsWith("postgresql://"))
{
    try
    {
        var uri = new Uri(rawConnectionString);
        var userInfo = uri.UserInfo.Split(':');

        var builder2 = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432, // Default to 5432 if no port specified
            Username = userInfo.Length > 0 ? userInfo[0] : "",
            Password = userInfo.Length > 1 ? userInfo[1] : "",
            Database = uri.LocalPath.TrimStart('/'),
            SslMode = SslMode.Require,
            IncludeErrorDetail = true
        };
        connectionString = builder2.ConnectionString;
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException($"Failed to parse DATABASE_URL: {ex.Message}. Raw value length: {rawConnectionString.Length}", ex);
    }
}
else
{
    connectionString = rawConnectionString;
}

// EF Core + PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// --------------------
// Authentication & Authorization (JWT)
// --------------------

var jwtSecret = builder.Configuration["Jwt:Secret"] ?? Environment.GetEnvironmentVariable("JWT_SECRET") ?? "default-development-secret-key-change-in-production";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "ExpenseManager";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "ExpenseManagerClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// --------------------
// Auto-create database schema (Render deployment)
// --------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    logger.LogWarning("===== STARTING DATABASE SCHEMA INITIALIZATION =====");

    try
    {
        logger.LogWarning("Testing database connection...");
        var canConnect = await db.Database.CanConnectAsync();
        logger.LogWarning($"Database connection test result: {canConnect}");

        if (canConnect)
        {
            logger.LogWarning("Creating database schema...");
            // EnsureCreated() creates all tables if they don't exist
            var created = db.Database.EnsureCreated();

            if (created)
            {
                logger.LogWarning("✓ Database schema created successfully!");
            }
            else
            {
                logger.LogWarning("Database schema already exists.");
            }

            // Verify tables were created
            logger.LogWarning("Verifying tables exist...");
            var usersCount = await db.Users.CountAsync();
            logger.LogWarning($"Users table exists. Current count: {usersCount}");
        }
        else
        {
            logger.LogError("Cannot connect to database!");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "FATAL: Failed to create database schema!");
        throw;
    }

    logger.LogWarning("===== DATABASE SCHEMA INITIALIZATION COMPLETE =====");
}

// --------------------
// Middleware
// --------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// --------------------
// Endpoints
// --------------------

// Health check (Render)
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// TEMP: DB check (remove after verification)
app.MapGet("/db-check", async (AppDbContext db) =>
{
    await db.Database.OpenConnectionAsync();
    return Results.Ok(new { db = "connected" });
});

// Controllers
app.MapControllers();

app.Run();
