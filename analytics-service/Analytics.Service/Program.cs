using Analytics.Application;
using Analytics.Application.Consumers;
using Analytics.Application.Jobs;
using Analytics.Application.Queries;
using Analytics.Infrastructure;
using Analytics.Service.Services;
using Ecommerce.Shared.Infrastructure;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.AddServiceDefaults("Analytics.Service");

    builder.Services.RegisterInfrastructure(builder.Configuration);
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSharedInfrastructure(builder.Configuration, bus =>
    {
        bus.AddConsumer<OrderPlacedConsumer>();
        bus.AddConsumer<OrderConfirmedConsumer>();
        bus.AddConsumer<OrderShippedConsumer>();
        bus.AddConsumer<OrderDeliveredConsumer>();
        bus.AddConsumer<OrderCancelledConsumer>();
        bus.AddConsumer<OrderReturnedConsumer>();
        bus.AddConsumer<PaymentRefundedConsumer>();
        bus.AddConsumer<UserRegisteredConsumer>();

        bus.AddEntityFrameworkOutbox<AnalyticsDbContext>(o =>
        {
            o.UsePostgres();
            o.UseBusOutbox();
        });
    });

    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(typeof(GetSalesOverviewQuery).Assembly);
    });

    builder.Services.AddHostedService<DailyStatsJob>();

    builder.Services.AddHealthChecks()
        .AddNpgSql(builder.Configuration.GetConnectionString("AnalyticsDb")!, name: "postgresql");

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
        db.Database.Migrate();
    }

    app.UseServiceDefaults();
    app.MapGrpcService<AnalyticsGrpcService>();

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
