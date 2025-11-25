using BMES.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace BMES.Infrastructure.Persistence
{
    public class BmesDbContext : DbContext
    {
        public BmesDbContext(DbContextOptions<BmesDbContext> options) : base(options)
        {
        }

        public DbSet<ProductionOrder> ProductionOrders { get; set; }
        public DbSet<AlarmEvent> AlarmEvents { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<RecipeParameter> RecipeParameters { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Тут можно добавить конфигурацию моделей, если потребуется
        }
    }
}
