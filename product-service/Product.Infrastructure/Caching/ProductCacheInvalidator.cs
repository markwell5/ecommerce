using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Product.Application.Caching;
using StackExchange.Redis;

namespace Product.Infrastructure.Caching;

public class ProductCacheInvalidator : IProductCacheInvalidator
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<ProductCacheInvalidator> _logger;

    public ProductCacheInvalidator(IConnectionMultiplexer redis, ILogger<ProductCacheInvalidator> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task InvalidateProductAsync(long productId, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();

        // Remove the specific product cache entry
        await db.KeyDeleteAsync($"product:item:{productId}");

        // Remove all query caches (paginated lists, searches) since they may contain this product
        await InvalidateQueryCacheAsync(db);

        _logger.LogInformation("Invalidated cache for product {ProductId}", productId);
    }

    public async Task InvalidateAllAsync(CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();

        await InvalidateQueryCacheAsync(db);

        // Also remove all individual product caches
        var server = _redis.GetServer(_redis.GetEndPoints()[0]);
        var keys = server.Keys(pattern: "product:*");
        foreach (var key in keys)
        {
            await db.KeyDeleteAsync(key);
        }

        _logger.LogInformation("Invalidated all product cache entries");
    }

    private async Task InvalidateQueryCacheAsync(IDatabase db)
    {
        var server = _redis.GetServer(_redis.GetEndPoints()[0]);
        var queryKeys = server.Keys(pattern: "product:query:*");
        foreach (var key in queryKeys)
        {
            await db.KeyDeleteAsync(key);
        }
    }
}
