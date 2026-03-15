using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Product.Application;
using StackExchange.Redis;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace Product.Integration.Tests;

public class ProductServiceFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .Build();

    private readonly RedisContainer _redis = new RedisBuilder()
        .WithImage("redis:7-alpine")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Replace PostgreSQL
            var dbDescriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<ProductDbContext>)
                         || d.ServiceType == typeof(ProductDbContext))
                .ToList();
            foreach (var d in dbDescriptors)
                services.Remove(d);

            services.AddDbContext<ProductDbContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString(),
                    b => b.MigrationsAssembly("Product.Infrastructure")));

            // Replace Redis
            var redisDescriptors = services
                .Where(d => d.ServiceType == typeof(IConnectionMultiplexer))
                .ToList();
            foreach (var d in redisDescriptors)
                services.Remove(d);

            services.AddSingleton<IConnectionMultiplexer>(_ =>
                ConnectionMultiplexer.Connect(_redis.GetConnectionString()));

            // Replace MassTransit with test harness
            services.AddMassTransitTestHarness();
        });

        builder.UseEnvironment("Development");
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await _redis.StartAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await _redis.DisposeAsync();
    }
}
