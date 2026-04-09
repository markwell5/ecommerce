using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Loyalty.Application;

namespace Loyalty.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection RegisterInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<LoyaltyDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("LoyaltyDb"),
                b => b.MigrationsAssembly(typeof(DependencyInjection).Assembly.FullName)));

        return services;
    }
}
