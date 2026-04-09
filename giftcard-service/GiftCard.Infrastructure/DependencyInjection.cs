using GiftCard.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GiftCard.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection RegisterInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<GiftCardDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("GiftCardDb"),
                b => b.MigrationsAssembly(typeof(DependencyInjection).Assembly.FullName)));

        return services;
    }
}
