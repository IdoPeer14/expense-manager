# SQL Scripts

Database-related SQL scripts.

## Files

- **`create_tables.sql`** - Manual table creation script (historical)

## Note

The application uses Entity Framework Core with automatic schema creation via `EnsureCreated()`. For production, use EF Core migrations instead:

```bash
# Create migration
dotnet ef migrations add MigrationName

# Apply migrations
dotnet ef database update
```

The SQL scripts in this folder are primarily for reference or manual database setup if needed.
