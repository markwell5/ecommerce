using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Return.Application;

namespace Return.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection RegisterInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ReturnDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("ReturnDb"),
                b => b.MigrationsAssembly(typeof(DependencyInjection).Assembly.FullName)));

        return services;
    }
}
