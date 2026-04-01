using Ecommerce.Shared.Protos;
using GraphQL.Api.Types;

namespace GraphQL.Api.DataLoaders;

public class StockBatchDataLoader : BatchDataLoader<long, StockLevel?>
{
    private readonly StockGrpc.StockGrpcClient _client;

    public StockBatchDataLoader(
        StockGrpc.StockGrpcClient client,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        _client = client;
    }

    protected override async Task<IReadOnlyDictionary<long, StockLevel?>> LoadBatchAsync(
        IReadOnlyList<long> keys,
        CancellationToken cancellationToken)
    {
        var results = new Dictionary<long, StockLevel?>();

        var tasks = keys.Select(async id =>
        {
            try
            {
                var reply = await _client.GetStockLevelAsync(
                    new GetStockLevelRequest { ProductId = id },
                    cancellationToken: cancellationToken);
                return (id, stock: (StockLevel?)new StockLevel
                {
                    ProductId = reply.ProductId,
                    AvailableQuantity = reply.AvailableQuantity,
                    ReservedQuantity = reply.ReservedQuantity
                });
            }
            catch
            {
                return (id, stock: (StockLevel?)null);
            }
        });

        foreach (var result in await Task.WhenAll(tasks))
        {
            results[result.id] = result.stock;
        }

        return results;
    }
}
