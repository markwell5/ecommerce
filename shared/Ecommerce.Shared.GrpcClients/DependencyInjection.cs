using Ecommerce.Shared.Protos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Shared.GrpcClients;

public static class DependencyInjection
{
    public static IServiceCollection AddProductGrpcClient(
        this IServiceCollection services, IConfiguration configuration)
    {
        var address = configuration["GrpcClients:ProductService"] ?? "http://product:8080";

        services.AddGrpcClient<ProductGrpc.ProductGrpcClient>(o =>
        {
            o.Address = new Uri(address);
        });

        return services;
    }

    public static IServiceCollection AddOrderGrpcClient(
        this IServiceCollection services, IConfiguration configuration)
    {
        var address = configuration["GrpcClients:OrderService"] ?? "http://order:8080";

        services.AddGrpcClient<OrderGrpc.OrderGrpcClient>(o =>
        {
            o.Address = new Uri(address);
        });

        return services;
    }

    public static IServiceCollection AddStockGrpcClient(
        this IServiceCollection services, IConfiguration configuration)
    {
        var address = configuration["GrpcClients:StockService"] ?? "http://stock:8080";

        services.AddGrpcClient<StockGrpc.StockGrpcClient>(o =>
        {
            o.Address = new Uri(address);
        });

        return services;
    }

    public static IServiceCollection AddUserGrpcClient(
        this IServiceCollection services, IConfiguration configuration)
    {
        var address = configuration["GrpcClients:UserService"] ?? "http://user:8080";

        services.AddGrpcClient<UserGrpc.UserGrpcClient>(o =>
        {
            o.Address = new Uri(address);
        });

        return services;
    }

    public static IServiceCollection AddPaymentGrpcClient(
        this IServiceCollection services, IConfiguration configuration)
    {
        var address = configuration["GrpcClients:PaymentService"] ?? "http://payment:8080";

        services.AddGrpcClient<PaymentGrpc.PaymentGrpcClient>(o =>
        {
            o.Address = new Uri(address);
        });

        return services;
    }

    public static IServiceCollection AddCartGrpcClient(
        this IServiceCollection services, IConfiguration configuration)
    {
        var address = configuration["GrpcClients:CartService"] ?? "http://cart:8080";

        services.AddGrpcClient<CartGrpc.CartGrpcClient>(o =>
        {
            o.Address = new Uri(address);
        });

        return services;
    }

    public static IServiceCollection AddCategoryGrpcClient(
        this IServiceCollection services, IConfiguration configuration)
    {
        var address = configuration["GrpcClients:ProductService"] ?? "http://product:8080";

        services.AddGrpcClient<CategoryGrpc.CategoryGrpcClient>(o =>
        {
            o.Address = new Uri(address);
        });

        return services;
    }

    public static IServiceCollection AddReviewGrpcClient(
        this IServiceCollection services, IConfiguration configuration)
    {
        var address = configuration["GrpcClients:ProductService"] ?? "http://product:8080";

        services.AddGrpcClient<ReviewGrpc.ReviewGrpcClient>(o =>
        {
            o.Address = new Uri(address);
        });

        return services;
    }

    public static IServiceCollection AddDiscountGrpcClient(
        this IServiceCollection services, IConfiguration configuration)
    {
        var address = configuration["GrpcClients:OrderService"] ?? "http://order:8080";

        services.AddGrpcClient<DiscountGrpc.DiscountGrpcClient>(o =>
        {
            o.Address = new Uri(address);
        });

        return services;
    }
}
