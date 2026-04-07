using Analytics.Application.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Application
{
    public class AnalyticsDbContext : DbContext
    {
        public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : base(options)
        {
        }

        public DbSet<AnalyticsOrder> AnalyticsOrders { get; set; }
        public DbSet<DailyStat> DailyStats { get; set; }
        public DbSet<CustomerRecord> CustomerRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();

            modelBuilder.Entity<AnalyticsOrder>(entity =>
            {
                entity.HasKey(e => e.OrderId);
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
                entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
                entity.Property(e => e.RefundAmount).HasPrecision(18, 2);
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.PlacedAt);
                entity.HasIndex(e => e.Status);
            });

            modelBuilder.Entity<DailyStat>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(e => e.Date).IsUnique();
                entity.Property(e => e.Revenue).HasPrecision(18, 2);
                entity.Property(e => e.AvgOrderValue).HasPrecision(18, 2);
            });

            modelBuilder.Entity<CustomerRecord>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.HasIndex(e => e.RegisteredAt);
            });
        }
    }
}
