using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stock.Application;

namespace Stock.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection RegisterInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<StockDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("StockDb"),
                b => b.MigrationsAssembly(typeof(DependencyInjection).Assembly.FullName)));

        return services;
    }
}
