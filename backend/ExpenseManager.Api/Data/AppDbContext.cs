using Microsoft.EntityFrameworkCore;

namespace ExpenseManager.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // DbSet<> יתווספו בשלב ה־Models
    }
}
