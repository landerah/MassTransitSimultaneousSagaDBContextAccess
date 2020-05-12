using Microsoft.EntityFrameworkCore;

namespace MassTransitDuplicateSagaHandlersIssue
{
    public class TestSagaDbContext : DbContext
    {
        public TestSagaDbContext(DbContextOptions<TestSagaDbContext> options) : base(options)
        {

        }

        public DbSet<TestSaga> TestSagas { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestSaga>()
                .HasIndex(e => e.CorrelationId);
            modelBuilder.Entity<TestSaga>()
                .Property(e => e.CurrentState)
                .HasMaxLength(64);
            modelBuilder.Entity<TestSaga>()
                .Property(e => e.Name)
                .HasMaxLength(20);
            modelBuilder.Entity<TestSaga>()
                .Property(e => e.CollectionId)
                .HasMaxLength(20);
            base.OnModelCreating(modelBuilder);
        }
    }
}