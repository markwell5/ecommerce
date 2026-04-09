using Ecommerce.Shared.Infrastructure;
using Ecommerce.Shared.Infrastructure.Validation;
using FluentValidation;
using GiftCard.Application;
using GiftCard.Application.Commands;
using GiftCard.Application.Consumers;
using GiftCard.Infrastructure;
using GiftCard.Service.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.AddServiceDefaults("GiftCard.Service");

    builder.Services.RegisterInfrastructure(builder.Configuration);
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSharedInfrastructure(builder.Configuration, bus =>
    {
        bus.AddConsumer<PaymentCompletedConsumer>();

        bus.AddEntityFrameworkOutbox<GiftCardDbContext>(o =>
        {
            o.UsePostgres();
            o.UseBusOutbox();
        });
    });

    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(typeof(PurchaseGiftCardCommand).Assembly);
        cfg.AddOpenBehavior(typeof(InputSanitizationBehavior<,>));
        cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    });
    builder.Services.AddValidatorsFromAssembly(typeof(PurchaseGiftCardCommand).Assembly);
    builder.Services.AddAutoMapper(cfg => { }, typeof(GiftCard.Application.MapperProfile).Assembly);

    builder.Services.AddHealthChecks()
        .AddNpgSql(builder.Configuration.GetConnectionString("GiftCardDb")!, name: "postgresql");

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<GiftCardDbContext>();
        db.Database.Migrate();
    }

    app.UseServiceDefaults();
    app.MapGrpcService<GiftCardGrpcService>();

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
