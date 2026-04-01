using Ecommerce.Shared.Protos;
using GraphQL.Api.Types;

namespace GraphQL.Api.DataLoaders;

public class ProductBatchDataLoader : BatchDataLoader<long, Product?>
{
    private readonly ProductGrpc.ProductGrpcClient _client;

    public ProductBatchDataLoader(
        ProductGrpc.ProductGrpcClient client,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _client = client;
    }

    protected override async Task<IReadOnlyDictionary<long, Product?>> LoadBatchAsync(
        IReadOnlyList<long> keys,
        CancellationToken cancellationToken)
    {
        var results = new Dictionary<long, Product?>();

        // gRPC doesn't have a batch-by-IDs endpoint, so we call individually
        // DataLoader still batches the calls within a single execution step
        var tasks = keys.Select(async id =>
        {
            try
            {
                var reply = await _client.GetProductAsync(
                    new GetProductRequest { Id = id },
                    cancellationToken: cancellationToken);
                return (id, product: MapProduct(reply));
            }
            catch
            {
                return (id, product: (Product?)null);
            }
        });

        foreach (var result in await Task.WhenAll(tasks))
        {
            results[result.id] = result.product;
        }

        return results;
    }

    private static Product MapProduct(ProductReply reply) => new()
    {
        Id = reply.Id,
        Name = reply.Name,
        Description = reply.Description,
        Category = reply.Category,
        Price = decimal.TryParse(reply.Price, out var p) ? p : 0
    };
}
