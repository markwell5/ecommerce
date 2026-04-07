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
        public DbSet<Entities.Coupon> Coupons { get; set; }

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
                entity.Property(e => e.CouponCode).HasMaxLength(50);
                entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            });

            modelBuilder.Entity<Entities.Coupon>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Code).IsUnique();
                entity.Property(e => e.DiscountType).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Value).HasPrecision(18, 2);
                entity.Property(e => e.MinOrderAmount).HasPrecision(18, 2);
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
            entity.Property(x => x.CouponCode).HasMaxLength(50);
            entity.Property(x => x.DiscountAmount).HasPrecision(18, 2);
        }
    }
}
