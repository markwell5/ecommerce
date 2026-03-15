using System;
using Ecommerce.Shared.Infrastructure.Idempotency;
using Ecommerce.Shared.Infrastructure.Logging;
using Ecommerce.Shared.Infrastructure.Middleware;
using Ecommerce.Shared.Infrastructure.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;

namespace Ecommerce.Shared.Infrastructure;

public static class ServiceDefaults
{
    public static WebApplicationBuilder AddServiceDefaults(
        this WebApplicationBuilder builder,
        string serviceName)
    {
        builder.Host.UseSerilog((context, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Service", serviceName)
                .WriteTo.Console(outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
        });

        builder.Services.AddCorsConfiguration(builder.Configuration);
        builder.Services.AddJwtAuthentication(builder.Configuration);
        builder.Services.AddRateLimiting(builder.Configuration);
        builder.Services.AddRequestResponseLogging(builder.Configuration);
        builder.Services.AddRequestSizeLimits(builder.Configuration);
        builder.Services.AddOpenTelemetryTracing(builder.Configuration, serviceName);
        builder.Services.AddIdempotency(builder.Configuration);

        builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
        builder.Services.AddProblemDetails();

        builder.Services.AddApiVersioningConfiguration();
        builder.Services.AddGrpc();
        builder.Services.AddControllers(options =>
        {
            options.Filters.AddService<IdempotencyFilter>();
        });

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = serviceName, Version = "v1" });
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

        return builder;
    }

    public static WebApplication UseServiceDefaults(this WebApplication app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<RequestResponseLoggingMiddleware>();
        app.UseRateLimiter();
        app.UseSerilogRequestLogging();
        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseCors(DependencyInjection.CorsPolicyName);
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapHealthChecks("/health");
        app.UseOpenTelemetryPrometheusScrapingEndpoint();

        return app;
    }
}
