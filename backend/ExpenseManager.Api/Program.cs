using ExpenseManager.Api.Data;
using Microsoft.EntityFrameworkCore;
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

// --------------------
// Database connection
// --------------------

// Prefer DATABASE_URL (Render), fallback to appsettings (local)
var rawConnectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DATABASE_URL");

if (string.IsNullOrWhiteSpace(rawConnectionString))
{
    throw new InvalidOperationException("Database connection string not configured.");
}

// Convert postgres URL -> Npgsql connection string if needed
var connectionString = rawConnectionString.StartsWith("postgres")
    ? new NpgsqlConnectionStringBuilder(rawConnectionString)
    {
        SslMode = SslMode.Require,
        TrustServerCertificate = true
    }.ConnectionString
    : rawConnectionString;

// EF Core + PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var app = builder.Build();

// --------------------
// Middleware
// --------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Auth will be added later (JWT)
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
