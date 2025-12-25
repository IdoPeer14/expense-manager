using Microsoft.EntityFrameworkCore;
using ExpenseManager.Api.Models;

namespace ExpenseManager.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Document> Documents { get; set; } = null!;
        public DbSet<Expense> Expenses { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();

                entity.HasMany(u => u.Documents)
                    .WithOne(d => d.User)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(u => u.Expenses)
                    .WithOne(e => e.User)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Document configuration
            modelBuilder.Entity<Document>(entity =>
            {
                entity.HasMany(d => d.Expenses)
                    .WithOne(e => e.Document)
                    .HasForeignKey(e => e.DocumentId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Expense configuration
            modelBuilder.Entity<Expense>(entity =>
            {
                entity.Property(e => e.Category)
                    .HasConversion<string>();
            });
        }
    }
}
