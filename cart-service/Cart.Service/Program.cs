using Cart.Application.Commands;
using Cart.Application.Mappings;
using Cart.Infrastructure;
using Cart.Service.Services;
using Ecommerce.Shared.GrpcClients;
using Ecommerce.Shared.Infrastructure;
using Ecommerce.Shared.Infrastructure.Validation;
using FluentValidation;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.AddServiceDefaults("Cart.Service");

    builder.Services.RegisterInfrastructure(builder.Configuration);
    builder.Services.AddSharedInfrastructure(builder.Configuration);

    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(typeof(AddToCartCommand).Assembly);
        cfg.AddOpenBehavior(typeof(InputSanitizationBehavior<,>));
        cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    });
    builder.Services.AddValidatorsFromAssembly(typeof(AddToCartCommand).Assembly);
    builder.Services.AddAutoMapper(cfg => { }, typeof(CartMappingProfile).Assembly);

    builder.Services.AddProductGrpcClient(builder.Configuration);

    builder.Services.AddHealthChecks()
        .AddRedis(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379", name: "redis");

    var app = builder.Build();

    app.UseServiceDefaults();
    app.MapGrpcService<CartGrpcService>();

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
