using MassTransit;
using Microsoft.EntityFrameworkCore;
using Subscription.Application.Entities;

namespace Subscription.Application
{
    public class SubscriptionDbContext : DbContext
    {
        public SubscriptionDbContext(DbContextOptions<SubscriptionDbContext> options) : base(options)
        {
        }

        public DbSet<Entities.Subscription> Subscriptions { get; set; }
        public DbSet<RenewalHistory> RenewalHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();

            modelBuilder.Entity<Entities.Subscription>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.CustomerId).HasMaxLength(100).IsRequired();
                entity.Property(e => e.ProductName).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Frequency).HasMaxLength(20).IsRequired();
                entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
                entity.Property(e => e.DiscountPercent).HasPrecision(18, 2);
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.NextRenewalAt);
                entity.HasIndex(e => e.Status);
            });

            modelBuilder.Entity<RenewalHistory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.OrderId).HasMaxLength(100);
                entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
                entity.Property(e => e.FailureReason).HasMaxLength(500);
                entity.HasIndex(e => e.SubscriptionId);
                entity.HasOne(e => e.Subscription)
                    .WithMany()
                    .HasForeignKey(e => e.SubscriptionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
