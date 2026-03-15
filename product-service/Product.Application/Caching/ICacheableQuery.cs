namespace Product.Application.Caching;

public interface ICacheableQuery
{
    string CacheKey { get; }
}
