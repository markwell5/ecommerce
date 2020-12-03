
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Microsoft.Extensions.Options;

namespace Product.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection RegisterInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<Settings>(configuration);
            var ms = new MongoSettings();

            services.AddTransient<MongoClient>(x =>
                new MongoClient(x.GetService<IOptions<Settings>>().Value.Mongo.ConnectionString));
            
            services.AddTransient<IProductRepository, MongoProductRepository>();
            return services;
        }
    }
}