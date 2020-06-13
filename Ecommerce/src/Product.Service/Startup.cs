using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Product.Service.Events;
using Product.Service.HostedServices;
using Product.Service.Repositories;
using System.Reflection;

namespace Product.Service
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMediatR(typeof(Startup));
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            services.Configure<Config.Settings>(Configuration);

            services.AddHostedService<DbMigratorHostedService>();
            services.AddHostedService<ConsumerHostedService>();
            services.AddSingleton<Consumer>();

            RegisterDependancies(services);

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void RegisterDependancies(IServiceCollection services)
        {
            services.AddTransient<IProductRespository, ProductRespository>();
        }
    }
}
