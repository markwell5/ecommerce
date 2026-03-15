using Cart.Application.Interfaces;
using Cart.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cart.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection RegisterInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis") ?? "localhost:6379";
            options.InstanceName = "cart:";
        });

        services.AddScoped<ICartRepository, RedisCartRepository>();
        services.AddScoped<IProductCatalogClient, ProductCatalogClient>();

        return services;
    }
}
