using System;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Product.Application;
using Product.Application.Caching;
using Product.Application.Search;
using Product.Infrastructure.Caching;
using Product.Infrastructure.Search;
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

        services.Configure<ElasticsearchSettings>(configuration.GetSection("Elasticsearch"));

        var elasticsearchUrl = configuration["Elasticsearch:Url"] ?? "http://localhost:9200";
        var settings = new ElasticsearchClientSettings(new Uri(elasticsearchUrl))
            .DefaultIndex(configuration["Elasticsearch:IndexName"] ?? "products");
        services.AddSingleton(new ElasticsearchClient(settings));
        services.AddSingleton<IProductSearchService, ElasticsearchProductSearchService>();

        return services;
    }
}
