using ExpenseManager.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --------------------
// Services
// --------------------

// Controllers (REST API)
builder.Services.AddControllers();

// Swagger (Development only)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Resolve connection string:
// 1. From appsettings.json (local)
// 2. From environment variable DATABASE_URL (Render)
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DATABASE_URL");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Database connection string not configured.");
}

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

// Authorization (JWT later)
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
