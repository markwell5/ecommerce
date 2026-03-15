using Ecommerce.Shared.Infrastructure;
using Ecommerce.Shared.Infrastructure.Validation;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Product.Application;
using Product.Application.Caching;
using Product.Application.Commands;
using Product.Infrastructure;
using Product.Service.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.AddServiceDefaults("Product.Service");

    builder.Services.RegisterInfrastructure(builder.Configuration);
    builder.Services.AddSharedInfrastructure(builder.Configuration, bus =>
    {
        bus.AddEntityFrameworkOutbox<ProductDbContext>(o =>
        {
            o.UsePostgres();
            o.UseBusOutbox();
        });
    });

    builder.Services.Configure<CacheSettings>(
        builder.Configuration.GetSection("CacheSettings"));

    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(typeof(CreateProductCommand).Assembly);
        cfg.AddOpenBehavior(typeof(CachingBehavior<,>));
        cfg.AddOpenBehavior(typeof(InputSanitizationBehavior<,>));
        cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    });
    builder.Services.AddValidatorsFromAssembly(typeof(CreateProductCommand).Assembly);
    builder.Services.AddAutoMapper(typeof(MapperProfile).Assembly);

    builder.Services.AddHealthChecks()
        .AddNpgSql(builder.Configuration.GetConnectionString("ProductDb")!, name: "postgresql")
        .AddRedis(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379", name: "redis");

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ProductDbContext>();
        db.Database.Migrate();
    }

    app.UseServiceDefaults();
    app.MapGrpcService<ProductGrpcService>();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
