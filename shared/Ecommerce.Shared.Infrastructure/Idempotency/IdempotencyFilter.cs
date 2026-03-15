using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ecommerce.Shared.Infrastructure.Idempotency;

public class IdempotencyFilter : IAsyncActionFilter
{
    private const string IdempotencyKeyHeader = "Idempotency-Key";
    private const string CachePrefix = "idempotency:";

    private readonly IDistributedCache _cache;
    private readonly IdempotencySettings _settings;
    private readonly ILogger<IdempotencyFilter> _logger;

    public IdempotencyFilter(
        IDistributedCache cache,
        IOptions<IdempotencySettings> settings,
        ILogger<IdempotencyFilter> logger)
    {
        _cache = cache;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var endpoint = context.ActionDescriptor.EndpointMetadata;
        var hasAttribute = false;
        foreach (var metadata in endpoint)
        {
            if (metadata is IdempotentEndpointAttribute)
            {
                hasAttribute = true;
                break;
            }
        }

        if (!hasAttribute)
        {
            await next();
            return;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(IdempotencyKeyHeader, out var idempotencyKey)
            || string.IsNullOrWhiteSpace(idempotencyKey))
        {
            await next();
            return;
        }

        var routePath = context.HttpContext.Request.Path.Value ?? "";
        var cacheKey = $"{CachePrefix}{routePath}:{idempotencyKey}";

        var cached = await _cache.GetAsync(cacheKey);
        if (cached != null)
        {
            _logger.LogInformation("Idempotent replay for key {IdempotencyKey} on {Path}", idempotencyKey.ToString(), routePath);
            var entry = IdempotencyCacheEntry.Deserialize(cached);
            context.HttpContext.Response.StatusCode = entry.StatusCode;
            context.HttpContext.Response.ContentType = entry.ContentType;
            context.Result = new IdempotentContentResult(entry);
            return;
        }

        var executedContext = await next();

        if (executedContext.Exception == null && executedContext.Result is ObjectResult objectResult)
        {
            var statusCode = objectResult.StatusCode ?? 200;
            var body = System.Text.Json.JsonSerializer.Serialize(objectResult.Value);
            var entry = new IdempotencyCacheEntry
            {
                StatusCode = statusCode,
                ContentType = "application/json",
                Body = body
            };

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(_settings.TtlHours)
            };

            await _cache.SetAsync(cacheKey, entry.Serialize(), options);
            _logger.LogDebug("Cached idempotent response for key {IdempotencyKey} on {Path}", idempotencyKey.ToString(), routePath);
        }
    }
}

internal class IdempotencyCacheEntry
{
    public int StatusCode { get; set; }
    public string ContentType { get; set; }
    public string Body { get; set; }

    public byte[] Serialize()
    {
        return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(this);
    }

    public static IdempotencyCacheEntry Deserialize(byte[] data)
    {
        return System.Text.Json.JsonSerializer.Deserialize<IdempotencyCacheEntry>(data);
    }
}

internal class IdempotentContentResult : IActionResult
{
    private readonly IdempotencyCacheEntry _entry;

    public IdempotentContentResult(IdempotencyCacheEntry entry)
    {
        _entry = entry;
    }

    public async Task ExecuteResultAsync(ActionContext context)
    {
        context.HttpContext.Response.StatusCode = _entry.StatusCode;
        context.HttpContext.Response.ContentType = _entry.ContentType;
        await context.HttpContext.Response.WriteAsync(_entry.Body);
    }
}
