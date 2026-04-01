using Ecommerce.Shared.Protos;
using GraphQL.Api.DataLoaders;

namespace GraphQL.Api.Types;

public class Product
{
    public long Id { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string Category { get; set; } = default!;
    public decimal Price { get; set; }
}

[ExtendObjectType(typeof(Product))]
public class ProductTypeExtensions
{
    public async Task<StockLevel?> GetStockLevel(
        [Parent] Product product,
        StockBatchDataLoader loader)
    {
        return await loader.LoadAsync(product.Id);
    }

    public async Task<ProductRating> GetRating(
        [Parent] Product product,
        ReviewGrpc.ReviewGrpcClient client)
    {
        var reply = await client.GetProductRatingAsync(new GetProductRatingRequest
        {
            ProductId = product.Id
        });

        return new ProductRating
        {
            ProductId = reply.ProductId,
            AverageRating = reply.AverageRating,
            ReviewCount = reply.ReviewCount
        };
    }
}
