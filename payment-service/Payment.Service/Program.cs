using Ecommerce.Shared.Infrastructure;
using Ecommerce.Shared.Infrastructure.Validation;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Payment.Application;
using Payment.Application.Commands;
using Payment.Application.Consumers;
using Payment.Application.Services;
using Payment.Infrastructure;
using Payment.Service.Services;
using Serilog;
using Stripe;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.AddServiceDefaults("Payment.Service");

    StripeConfiguration.ApiKey = builder.Configuration["StripeSettings:SecretKey"];

    builder.Services.RegisterInfrastructure(builder.Configuration);

    var redisConnection = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnection;
        options.InstanceName = "payment:";
    });

    builder.Services.AddSharedInfrastructure(builder.Configuration, bus =>
    {
        bus.AddConsumer<ProcessPaymentConsumer>();
        bus.AddConsumer<RefundPaymentConsumer>();
        bus.AddConsumer<ProcessPaymentFaultConsumer>();
        bus.AddConsumer<RefundPaymentFaultConsumer>();

        bus.AddEntityFrameworkOutbox<PaymentDbContext>(o =>
        {
            o.UsePostgres();
            o.UseBusOutbox();
        });
    });

    builder.Services.AddScoped<StripePaymentGateway>();
    builder.Services.AddScoped<IPaymentGateway>(sp =>
        new ResilientPaymentGateway(
            sp.GetRequiredService<StripePaymentGateway>(),
            sp.GetRequiredService<ILogger<ResilientPaymentGateway>>()));

    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(typeof(RefundPaymentCommand).Assembly);
        cfg.AddOpenBehavior(typeof(InputSanitizationBehavior<,>));
        cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    });
    builder.Services.AddValidatorsFromAssembly(typeof(RefundPaymentCommand).Assembly);
    builder.Services.AddAutoMapper(typeof(Payment.Application.MapperProfile).Assembly);

    builder.Services.AddHealthChecks()
        .AddNpgSql(builder.Configuration.GetConnectionString("PaymentDb")!, name: "postgresql");

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
        db.Database.Migrate();
    }

    app.UseServiceDefaults();
    app.MapGrpcService<PaymentGrpcService>();

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
