using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Product.Application;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options)
        : base(options)
    {
    }

    public DbSet<Entities.Product> Products => Set<Entities.Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();

        modelBuilder.Entity<Entities.Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.Price).HasPrecision(18, 2);

            if (Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL")
            {
                entity.HasGeneratedTsVectorColumn(
                        e => e.SearchVector,
                        "english",
                        e => new { e.Name, e.Description, e.Category })
                    .HasIndex(e => e.SearchVector)
                    .HasMethod("GIN");
            }
            else
            {
                entity.Ignore(e => e.SearchVector);
            }

            entity.HasIndex(e => e.Category);
        });
    }
}
