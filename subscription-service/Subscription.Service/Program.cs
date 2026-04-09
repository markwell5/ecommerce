using Ecommerce.Shared.Infrastructure;
using Ecommerce.Shared.Infrastructure.Validation;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Subscription.Application;
using Subscription.Application.Commands;
using Subscription.Infrastructure;
using Subscription.Service.Services;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.AddServiceDefaults("Subscription.Service");

    builder.Services.RegisterInfrastructure(builder.Configuration);
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSharedInfrastructure(builder.Configuration, bus =>
    {
        bus.AddEntityFrameworkOutbox<SubscriptionDbContext>(o =>
        {
            o.UsePostgres();
            o.UseBusOutbox();
        });
    });

    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(typeof(CreateSubscriptionCommand).Assembly);
        cfg.AddOpenBehavior(typeof(InputSanitizationBehavior<,>));
        cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    });
    builder.Services.AddValidatorsFromAssembly(typeof(CreateSubscriptionCommand).Assembly);
    builder.Services.AddAutoMapper(cfg => { }, typeof(Subscription.Application.MapperProfile).Assembly);

    builder.Services.AddHealthChecks()
        .AddNpgSql(builder.Configuration.GetConnectionString("SubscriptionDb")!, name: "postgresql");

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<SubscriptionDbContext>();
        db.Database.Migrate();
    }

    app.UseServiceDefaults();
    app.MapGrpcService<SubscriptionGrpcService>();

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
