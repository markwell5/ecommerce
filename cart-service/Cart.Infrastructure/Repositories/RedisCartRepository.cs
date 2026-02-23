using System.Text.Json;
using Cart.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;

namespace Cart.Infrastructure.Repositories;

public class RedisCartRepository : ICartRepository
{
    private readonly IDistributedCache _cache;
    private readonly DistributedCacheEntryOptions _cacheOptions;

    public RedisCartRepository(IDistributedCache cache, IConfiguration configuration)
    {
        _cache = cache;

        var expiryHours = configuration.GetValue("CartSettings:ExpiryHours", 24);
        _cacheOptions = new DistributedCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromHours(expiryHours)
        };
    }

    public async Task<Application.Models.Cart?> GetCartAsync(string cartId)
    {
        var json = await _cache.GetStringAsync(CartKey(cartId));
        return json is null ? null : JsonSerializer.Deserialize<Application.Models.Cart>(json);
    }

    public async Task SaveCartAsync(Application.Models.Cart cart)
    {
        var json = JsonSerializer.Serialize(cart);
        await _cache.SetStringAsync(CartKey(cart.Id), json, _cacheOptions);
    }

    public async Task DeleteCartAsync(string cartId)
    {
        await _cache.RemoveAsync(CartKey(cartId));
    }

    private static string CartKey(string cartId) => $"cart:{cartId}";
}
