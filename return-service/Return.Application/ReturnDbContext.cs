using MassTransit;
using Microsoft.EntityFrameworkCore;
using Return.Application.Entities;

namespace Return.Application
{
    public class ReturnDbContext : DbContext
    {
        public ReturnDbContext(DbContextOptions<ReturnDbContext> options) : base(options)
        {
        }

        public DbSet<ReturnRequest> ReturnRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();

            modelBuilder.Entity<ReturnRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(e => e.RmaNumber).IsUnique();
                entity.Property(e => e.RmaNumber).HasMaxLength(20).IsRequired();
                entity.Property(e => e.Reason).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
                entity.Property(e => e.Resolution).HasMaxLength(20);
                entity.Property(e => e.RefundAmount).HasPrecision(18, 2);
                entity.Property(e => e.RestockingFee).HasPrecision(18, 2);
                entity.Property(e => e.InspectionNotes).HasMaxLength(1000);
                entity.Property(e => e.AdminNotes).HasMaxLength(1000);
                entity.HasIndex(e => e.OrderId);
                entity.HasIndex(e => e.CustomerId);
            });
        }
    }
}
