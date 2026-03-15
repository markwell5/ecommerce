using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Product.Application;
using Product.Application.Caching;
using Product.Infrastructure.Caching;
using StackExchange.Redis;

namespace Product.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection RegisterInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ProductDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("ProductDb"),
                b => b.MigrationsAssembly(typeof(DependencyInjection).Assembly.FullName)));

        var redisConnection = configuration.GetConnectionString("Redis") ?? "localhost:6379";

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(redisConnection));

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = "product:";
        });

        services.AddScoped<IProductCacheInvalidator, ProductCacheInvalidator>();

        return services;
    }
}
