using BMES.Core.Models;
using Microsoft.EntityFrameworkCore;
using BMES.Infrastructure.Persistence;

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
        public DbSet<MaterialLot> MaterialLots { get; set; }
        public DbSet<ProcessWorkflow> ProcessWorkflows { get; set; }
        public DbSet<WorkflowStep> WorkflowSteps { get; set; }
        public DbSet<EquipmentStateLog> EquipmentStateLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configure TPH for WorkflowStep
            modelBuilder.Entity<WorkflowStep>()
                .HasDiscriminator<string>("StepType")
                .HasValue<ControlStep>("ControlStep")
                .HasValue<MonitorStep>("MonitorStep")
                .HasValue<OperatorInstructionStep>("OperatorInstructionStep")
                .HasValue<DataCollectionStep>("DataCollectionStep");

            modelBuilder.Entity<ControlStep>()
                .Property(cs => cs.Value)
                .HasConversion<ObjectToJsonConverter>();

            modelBuilder.Entity<RecipeParameter>()
                .Property(rp => rp.Value)
                .HasConversion<ObjectToJsonConverter>();

        }
    }
}
