using Cart.Application.Commands;
using Cart.Application.Mappings;
using Cart.Infrastructure;
using Cart.Service.Services;
using Ecommerce.Shared.GrpcClients;
using Ecommerce.Shared.Infrastructure;
using Ecommerce.Shared.Infrastructure.Logging;
using Ecommerce.Shared.Infrastructure.Middleware;
using Ecommerce.Shared.Infrastructure.Validation;
using FluentValidation;
using Microsoft.OpenApi.Models;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Service", "Cart.Service")
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
    });

    builder.Services.RegisterInfrastructure(builder.Configuration);
    builder.Services.AddSharedInfrastructure(builder.Configuration);
    builder.Services.AddRateLimiting(builder.Configuration);
    builder.Services.AddRequestResponseLogging(builder.Configuration);
    builder.Services.AddRequestSizeLimits(builder.Configuration);
    builder.Services.AddOpenTelemetryTracing(builder.Configuration, "Cart.Service");
    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(typeof(AddToCartCommand).Assembly);
        cfg.AddOpenBehavior(typeof(InputSanitizationBehavior<,>));
        cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    });
    builder.Services.AddValidatorsFromAssembly(typeof(AddToCartCommand).Assembly);
    builder.Services.AddAutoMapper(cfg => { }, typeof(CartMappingProfile).Assembly);

    builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
    builder.Services.AddProblemDetails();

    builder.Services.AddHealthChecks()
        .AddRedis(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379", name: "redis");

    builder.Services.AddApiVersioningConfiguration();
    builder.Services.AddGrpc();
    builder.Services.AddProductGrpcClient(builder.Configuration);
    builder.Services.AddControllers();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Cart.Service", Version = "v1" });
    });

    var app = builder.Build();

    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<RequestResponseLoggingMiddleware>();
    app.UseRateLimiter();
    app.UseSerilogRequestLogging();
    app.UseExceptionHandler();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cart.Service v1"));
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapGrpcService<CartGrpcService>();
    app.MapControllers();
    app.MapHealthChecks("/health");
    app.UseOpenTelemetryPrometheusScrapingEndpoint();

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
