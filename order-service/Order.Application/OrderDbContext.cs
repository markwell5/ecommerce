using System.Collections.Generic;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Order.Application.Entities;

namespace Order.Application
{
    public class OrderDbContext : SagaDbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
        {
        }

        public DbSet<Entities.Order> Orders { get; set; }
        public DbSet<OrderEvent> OrderEvents { get; set; }

        protected override IEnumerable<ISagaClassMap> Configurations
        {
            get { yield return new OrderSagaStateMap(); }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();

            modelBuilder.Entity<Entities.Order>(entity =>
            {
                entity.HasKey(e => e.OrderId);
                entity.Property(e => e.CustomerId).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
                entity.Property(e => e.ItemsJson).HasColumnType("text");
            });

            modelBuilder.Entity<OrderEvent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.EventName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Payload).HasColumnType("text");
                entity.HasIndex(e => e.OrderId);
            });
        }
    }

    public class OrderSagaStateMap : SagaClassMap<OrderSagaState>
    {
        protected override void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<OrderSagaState> entity, ModelBuilder model)
        {
            entity.Property(x => x.CurrentState).HasMaxLength(64);
            entity.Property(x => x.CustomerId).HasMaxLength(200);
            entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
            entity.Property(x => x.ItemsJson).HasColumnType("text");
        }
    }
}
