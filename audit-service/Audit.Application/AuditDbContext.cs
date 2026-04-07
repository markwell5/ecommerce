using Audit.Application.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Audit.Application
{
    public class AuditDbContext : DbContext
    {
        public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options)
        {
        }

        public DbSet<AuditEntry> AuditEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();

            modelBuilder.Entity<AuditEntry>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Service).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Action).HasMaxLength(100).IsRequired();
                entity.Property(e => e.ActorId).HasMaxLength(100);
                entity.Property(e => e.ActorType).HasMaxLength(20);
                entity.Property(e => e.EntityType).HasMaxLength(100);
                entity.Property(e => e.EntityId).HasMaxLength(100);
                entity.Property(e => e.CorrelationId).HasMaxLength(100);
                entity.Property(e => e.IpAddress).HasMaxLength(50);
                entity.Property(e => e.Hash).HasMaxLength(64).IsRequired();
                entity.Property(e => e.PreviousHash).HasMaxLength(64);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.ActorId);
                entity.HasIndex(e => e.EntityType);
                entity.HasIndex(e => e.Service);
                entity.HasIndex(e => e.CorrelationId);
            });
        }
    }
}
