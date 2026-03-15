using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Stock.Application;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace Stock.Integration.Tests;

public class StockServiceFactory : WebApplicationFactory<Program>, IAsyncLifetime
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
            var dbDescriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<StockDbContext>)
                         || d.ServiceType == typeof(StockDbContext))
                .ToList();
            foreach (var d in dbDescriptors)
                services.Remove(d);

            services.AddDbContext<StockDbContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString(),
                    b => b.MigrationsAssembly("Stock.Infrastructure")));

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
