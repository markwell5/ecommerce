using Audit.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Audit.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection RegisterInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AuditDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("AuditDb"),
                b => b.MigrationsAssembly(typeof(DependencyInjection).Assembly.FullName)));

        return services;
    }
}
