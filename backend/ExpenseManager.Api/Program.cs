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
        var builder2 = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port,
            Username = uri.UserInfo.Split(':')[0],
            Password = uri.UserInfo.Split(':')[1],
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
// Auto-apply migrations (Render deployment)
// --------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
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
