using Ecommerce.Shared.Infrastructure;
using Ecommerce.Shared.Infrastructure.Middleware;
using Ecommerce.Shared.Infrastructure.Validation;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
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

    builder.Host.UseSerilog((context, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Service", "Payment.Service")
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
    });

    StripeConfiguration.ApiKey = builder.Configuration["StripeSettings:SecretKey"];

    builder.Services.RegisterInfrastructure(builder.Configuration);
    builder.Services.AddCorsConfiguration(builder.Configuration);
    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddRateLimiting(builder.Configuration);
    builder.Services.AddIdempotency(builder.Configuration);

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

        bus.AddEntityFrameworkOutbox<PaymentDbContext>(o =>
        {
            o.UsePostgres();
            o.UseBusOutbox();
        });
    });

    builder.Services.AddScoped<IPaymentGateway, StripePaymentGateway>();

    builder.Services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(typeof(RefundPaymentCommand).Assembly);
        cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    });
    builder.Services.AddValidatorsFromAssembly(typeof(RefundPaymentCommand).Assembly);
    builder.Services.AddAutoMapper(typeof(Payment.Application.MapperProfile).Assembly);

    builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
    builder.Services.AddProblemDetails();

    builder.Services.AddHealthChecks()
        .AddNpgSql(builder.Configuration.GetConnectionString("PaymentDb")!, name: "postgresql");

    builder.Services.AddApiVersioningConfiguration();
    builder.Services.AddGrpc();
    builder.Services.AddControllers(options =>
    {
        options.Filters.AddService<Ecommerce.Shared.Infrastructure.Idempotency.IdempotencyFilter>();
    });
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Payment.Service", Version = "v1" });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
        db.Database.Migrate();
    }

    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseRateLimiter();
    app.UseSerilogRequestLogging();
    app.UseExceptionHandler();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Payment.Service v1"));
    }

    app.UseHttpsRedirection();
    app.UseCors(Ecommerce.Shared.Infrastructure.DependencyInjection.CorsPolicyName);
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapGrpcService<PaymentGrpcService>();
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
