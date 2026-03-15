using Ecommerce.Shared.Infrastructure;
using Ecommerce.Shared.Infrastructure.Validation;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Stock.Application;
using Stock.Application.Commands;
using Stock.Application.Consumers;
using Stock.Infrastructure;
using Stock.Service.Services;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.AddServiceDefaults("Stock.Service");

    builder.Services.RegisterInfrastructure(builder.Configuration);

    var redisConnection = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
        options.InstanceName = "stock:";
    });

    builder.Services.AddSharedInfrastructure(builder.Configuration, bus =>
    {
        bus.AddConsumer<ReserveStockConsumer>();
        bus.AddConsumer<ReleaseStockConsumer>();
        bus.AddConsumer<ProductCreatedConsumer>();
        bus.AddConsumer<ReserveStockFaultConsumer>();
        bus.AddConsumer<ReleaseStockFaultConsumer>();

        bus.AddEntityFrameworkOutbox<StockDbContext>(o =>
        {
            o.UsePostgres();
            o.UseBusOutbox();
        });
    });

    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(typeof(UpdateStockCommand).Assembly);
        cfg.AddOpenBehavior(typeof(InputSanitizationBehavior<,>));
        cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    });
    builder.Services.AddValidatorsFromAssembly(typeof(UpdateStockCommand).Assembly);
    builder.Services.AddAutoMapper(typeof(Stock.Application.MapperProfile).Assembly);

    builder.Services.AddHealthChecks()
        .AddNpgSql(builder.Configuration.GetConnectionString("StockDb")!, name: "postgresql");

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<StockDbContext>();
        db.Database.Migrate();
    }

    app.UseServiceDefaults();
    app.MapGrpcService<StockGrpcService>();

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
