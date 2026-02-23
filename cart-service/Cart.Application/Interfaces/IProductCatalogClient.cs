namespace Cart.Application.Interfaces;

public interface IProductCatalogClient
{
    Task<ProductInfo?> GetProductAsync(long productId, CancellationToken cancellationToken = default);
}

public record ProductInfo(long Id, string Name, decimal Price);
