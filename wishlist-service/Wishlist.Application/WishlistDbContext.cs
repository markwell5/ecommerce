using MassTransit;
using Microsoft.EntityFrameworkCore;
using Wishlist.Application.Entities;

namespace Wishlist.Application
{
    public class WishlistDbContext : DbContext
    {
        public WishlistDbContext(DbContextOptions<WishlistDbContext> options) : base(options)
        {
        }

        public DbSet<Entities.Wishlist> Wishlists { get; set; }
        public DbSet<WishlistItem> WishlistItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();

            modelBuilder.Entity<Entities.Wishlist>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.CustomerId).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.ShareToken).IsUnique();
            });

            modelBuilder.Entity<WishlistItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(e => new { e.WishlistId, e.ProductId }).IsUnique();
                entity.HasIndex(e => e.ProductId);
                entity.HasOne(e => e.Wishlist)
                    .WithMany(w => w.Items)
                    .HasForeignKey(e => e.WishlistId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
