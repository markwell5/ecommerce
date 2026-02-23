using Ecommerce.Shared.Infrastructure;
using Ecommerce.Shared.Infrastructure.Middleware;
using Ecommerce.Shared.Infrastructure.Validation;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using Stock.Application;
using Stock.Application.Commands;
using Stock.Application.Consumers;
using Stock.Infrastructure;

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
            .Enrich.WithProperty("Service", "Stock.Service")
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
    });

    builder.Services.RegisterInfrastructure(builder.Configuration);
    builder.Services.AddCorsConfiguration(builder.Configuration);
    builder.Services.AddRateLimiting(builder.Configuration);
    builder.Services.AddSharedInfrastructure(builder.Configuration, bus =>
    {
        bus.AddConsumer<ReserveStockConsumer>();
        bus.AddConsumer<ReleaseStockConsumer>();
        bus.AddConsumer<ProductCreatedConsumer>();
    });

    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(typeof(UpdateStockCommand).Assembly);
        cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    });
    builder.Services.AddValidatorsFromAssembly(typeof(UpdateStockCommand).Assembly);
    builder.Services.AddAutoMapper(typeof(Stock.Application.MapperProfile).Assembly);

    builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
    builder.Services.AddProblemDetails();

    builder.Services.AddHealthChecks()
        .AddNpgSql(builder.Configuration.GetConnectionString("StockDb")!, name: "postgresql");

    builder.Services.AddControllers();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Stock.Service", Version = "v1" });
    });

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<StockDbContext>();
        db.Database.Migrate();
    }

    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseRateLimiter();
    app.UseSerilogRequestLogging();
    app.UseExceptionHandler();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Stock.Service v1"));
    }

    app.UseHttpsRedirection();
    app.UseCors(Ecommerce.Shared.Infrastructure.DependencyInjection.CorsPolicyName);
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health");

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
