using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Product.Application.Caching;

public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheableQuery
{
    private readonly IDistributedCache _cache;
    private readonly CacheSettings _settings;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(
        IDistributedCache cache,
        IOptions<CacheSettings> settings,
        ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var cacheKey = request.CacheKey;

        var cached = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (cached is not null)
        {
            _logger.LogDebug("Cache hit for {CacheKey}", cacheKey);
            return JsonSerializer.Deserialize<TResponse>(cached)!;
        }

        _logger.LogDebug("Cache miss for {CacheKey}", cacheKey);
        var response = await next();

        if (response is not null)
        {
            var ttl = request is Queries.GetProductQuery
                ? _settings.ProductTtlSeconds
                : _settings.QueryTtlSeconds;

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(ttl)
            };

            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(response),
                options,
                cancellationToken);
        }

        return response;
    }
}
