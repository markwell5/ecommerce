using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Shared.Infrastructure.Audit
{
    public static class AuditServiceCollectionExtensions
    {
        public static IServiceCollection AddAuditPublisher(this IServiceCollection services, string serviceName)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<IAuditPublisher>(sp =>
                new AuditPublisher(
                    sp.GetRequiredService<MassTransit.IPublishEndpoint>(),
                    sp.GetRequiredService<Microsoft.AspNetCore.Http.IHttpContextAccessor>(),
                    sp.GetRequiredService<ILogger<AuditPublisher>>(),
                    serviceName));

            return services;
        }
    }
}
