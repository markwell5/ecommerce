using Ecommerce.Shared.GrpcClients;
using Ecommerce.Shared.Infrastructure;
using GraphQL.Api.DataLoaders;
using GraphQL.Api.Types;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.AddServiceDefaults("GraphQL.Api");

    // Register gRPC clients for all backend services
    builder.Services.AddProductGrpcClient(builder.Configuration);
    builder.Services.AddOrderGrpcClient(builder.Configuration);
    builder.Services.AddStockGrpcClient(builder.Configuration);
    builder.Services.AddUserGrpcClient(builder.Configuration);
    builder.Services.AddPaymentGrpcClient(builder.Configuration);
    builder.Services.AddCartGrpcClient(builder.Configuration);
    builder.Services.AddCategoryGrpcClient(builder.Configuration);
    builder.Services.AddReviewGrpcClient(builder.Configuration);
    builder.Services.AddDiscountGrpcClient(builder.Configuration);
    builder.Services.AddReturnsGrpcClient(builder.Configuration);
    builder.Services.AddAuditGrpcClient(builder.Configuration);
    builder.Services.AddAnalyticsGrpcClient(builder.Configuration);
    builder.Services.AddLoyaltyGrpcClient(builder.Configuration);
    builder.Services.AddGiftCardGrpcClient(builder.Configuration);

    // Configure Hot Chocolate GraphQL server
    builder.Services
        .AddGraphQLServer()
        .AddAuthorization()
        .AddQueryType<Query>()
        .AddMutationType<Mutation>()
        .AddTypeExtension<ProductTypeExtensions>()
        .AddTypeExtension<OrderTypeExtensions>()
        .AddTypeExtension<CartItemTypeExtensions>()
        .AddDataLoader<ProductBatchDataLoader>()
        .AddDataLoader<StockBatchDataLoader>()
        .AddDataLoader<PaymentByOrderDataLoader>()
        .AddDataLoader<UserDataLoader>();

    builder.Services.AddHealthChecks();

    var app = builder.Build();

    app.UseServiceDefaults();
    app.MapGraphQL();

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
