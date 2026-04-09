using MassTransit;
using Microsoft.EntityFrameworkCore;
using Loyalty.Application.Entities;

namespace Loyalty.Application
{
    public class LoyaltyDbContext : DbContext
    {
        public LoyaltyDbContext(DbContextOptions<LoyaltyDbContext> options) : base(options)
        {
        }

        public DbSet<LoyaltyAccount> LoyaltyAccounts { get; set; }
        public DbSet<PointsTransaction> PointsTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();

            modelBuilder.Entity<LoyaltyAccount>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(e => e.CustomerId).IsUnique();
                entity.Property(e => e.CustomerId).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Tier).HasMaxLength(20).IsRequired();
                entity.Property(e => e.AnnualSpend).HasPrecision(18, 2);
            });

            modelBuilder.Entity<PointsTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Type).HasMaxLength(20).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500).IsRequired();
                entity.Property(e => e.CustomerId).HasMaxLength(100).IsRequired();
                entity.HasIndex(e => e.LoyaltyAccountId);
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.OrderId);
            });
        }
    }
}
