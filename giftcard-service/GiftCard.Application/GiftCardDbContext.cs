using GiftCard.Application.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace GiftCard.Application
{
    public class GiftCardDbContext : DbContext
    {
        public GiftCardDbContext(DbContextOptions<GiftCardDbContext> options) : base(options)
        {
        }

        public DbSet<GiftCardEntity> GiftCards { get; set; }
        public DbSet<GiftCardTransaction> GiftCardTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();

            modelBuilder.Entity<GiftCardEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(e => e.Code).IsUnique();
                entity.Property(e => e.Code).HasMaxLength(19).IsRequired();
                entity.Property(e => e.InitialValue).HasPrecision(18, 2);
                entity.Property(e => e.CurrentBalance).HasPrecision(18, 2);
                entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
                entity.Property(e => e.RecipientEmail).HasMaxLength(256);
                entity.Property(e => e.PersonalMessage).HasMaxLength(500);
                entity.Property(e => e.PurchasedByCustomerId).HasMaxLength(100).IsRequired();
                entity.HasIndex(e => e.PurchasedByCustomerId);
            });

            modelBuilder.Entity<GiftCardTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Type).HasMaxLength(20).IsRequired();
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.BalanceAfter).HasPrecision(18, 2);
                entity.Property(e => e.Description).HasMaxLength(500).IsRequired();
                entity.HasIndex(e => e.GiftCardId);
                entity.HasIndex(e => e.OrderId);
            });
        }
    }
}
