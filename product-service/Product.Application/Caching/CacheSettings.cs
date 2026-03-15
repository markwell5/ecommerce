namespace Product.Application.Caching;

public class CacheSettings
{
    public int ProductTtlSeconds { get; set; } = 300;
    public int QueryTtlSeconds { get; set; } = 120;
}
