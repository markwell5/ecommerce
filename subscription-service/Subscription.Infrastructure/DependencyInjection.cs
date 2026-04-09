using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Subscription.Application;

namespace Subscription.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection RegisterInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SubscriptionDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("SubscriptionDb"),
                b => b.MigrationsAssembly(typeof(DependencyInjection).Assembly.FullName)));
        return services;
    }
}
