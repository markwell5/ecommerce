using System.Security.Claims;
using Ecommerce.Shared.Protos;
using GraphQL.Api.DataLoaders;
using HotChocolate.Authorization;

namespace GraphQL.Api.Types;

public class Query
{
    // ── Products ──────────────────────────────────────

    public async Task<Product?> GetProduct(
        long id,
        ProductBatchDataLoader loader)
    {
        return await loader.LoadAsync(id);
    }

    public async Task<ProductConnection> GetProducts(
        int page,
        int pageSize,
        ProductGrpc.ProductGrpcClient client)
    {
        var reply = await client.GetProductsAsync(new GetProductsRequest
        {
            Page = page,
            PageSize = pageSize
        });

        return new ProductConnection
        {
            Items = reply.Products.Select(p => new Product
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Category = p.Category,
                Price = decimal.TryParse(p.Price, out var price) ? price : 0
            }).ToList(),
            TotalCount = reply.TotalCount,
            Page = reply.Page,
            PageSize = reply.PageSize
        };
    }

    // ── Categories ────────────────────────────────────

    public async Task<List<Category>> GetCategories(
        CategoryGrpc.CategoryGrpcClient client)
    {
        var reply = await client.GetCategoriesAsync(new GetCategoriesRequest());
        return reply.Categories.Select(MapCategory).ToList();
    }

    public async Task<Category?> GetCategoryBySlug(
        string slug,
        CategoryGrpc.CategoryGrpcClient client)
    {
        var reply = await client.GetCategoryBySlugAsync(new GetCategoryBySlugRequest { Slug = slug });
        return MapCategory(reply);
    }

    // ── Reviews ──────────────────────────────────────

    public async Task<ReviewConnection> GetProductReviews(
        long productId,
        int page,
        int pageSize,
        ReviewGrpc.ReviewGrpcClient client)
    {
        var reply = await client.GetProductReviewsAsync(new GetProductReviewsRequest
        {
            ProductId = productId,
            Page = page,
            PageSize = pageSize
        });

        return new ReviewConnection
        {
            Items = reply.Reviews.Select(r => new Review
            {
                Id = r.Id,
                ProductId = r.ProductId,
                CustomerId = r.CustomerId,
                Rating = r.Rating,
                Title = r.Title,
                Body = r.Body,
                CreatedAt = r.CreatedAt
            }).ToList(),
            TotalCount = reply.TotalCount,
            Page = reply.Page,
            PageSize = reply.PageSize
        };
    }

    public async Task<ProductRating> GetProductRating(
        long productId,
        ReviewGrpc.ReviewGrpcClient client)
    {
        var reply = await client.GetProductRatingAsync(new GetProductRatingRequest
        {
            ProductId = productId
        });

        return new ProductRating
        {
            ProductId = reply.ProductId,
            AverageRating = reply.AverageRating,
            ReviewCount = reply.ReviewCount
        };
    }

    // ── Orders ───────────────────────────────────────

    [Authorize]
    public async Task<Order?> GetOrder(
        string orderId,
        OrderGrpc.OrderGrpcClient client)
    {
        var reply = await client.GetOrderAsync(new GetOrderRequest { OrderId = orderId });
        return MapOrder(reply);
    }

    // ── Stock ────────────────────────────────────────

    public async Task<StockLevel?> GetStockLevel(
        long productId,
        StockBatchDataLoader loader)
    {
        return await loader.LoadAsync(productId);
    }

    // ── Users ────────────────────────────────────────

    [Authorize]
    public async Task<User?> GetMe(
        ClaimsPrincipal claimsPrincipal,
        UserDataLoader loader)
    {
        var userId = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return null;
        return await loader.LoadAsync(userId);
    }

    [Authorize]
    public async Task<List<Address>> GetMyAddresses(
        ClaimsPrincipal claimsPrincipal,
        UserGrpc.UserGrpcClient client)
    {
        var userId = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return [];

        var reply = await client.GetAddressesAsync(new GetAddressesRequest { UserId = userId });
        return reply.Addresses.Select(a => new Address
        {
            Id = a.Id,
            Line1 = a.Line1,
            Line2 = a.Line2,
            City = a.City,
            County = a.County,
            PostCode = a.PostCode,
            Country = a.Country,
            IsDefault = a.IsDefault
        }).ToList();
    }

    // ── Payments ─────────────────────────────────────

    [Authorize]
    public async Task<Payment?> GetPaymentByOrder(
        string orderId,
        PaymentByOrderDataLoader loader)
    {
        return await loader.LoadAsync(orderId);
    }

    [Authorize]
    public async Task<List<Payment>> GetMyPayments(
        ClaimsPrincipal claimsPrincipal,
        PaymentGrpc.PaymentGrpcClient client)
    {
        var userId = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return [];

        var reply = await client.GetPaymentsByCustomerAsync(
            new GetPaymentsByCustomerRequest { CustomerId = userId });

        return reply.Payments.Select(p => new Payment
        {
            Id = p.Id,
            OrderId = p.OrderId,
            CustomerId = p.CustomerId,
            Amount = decimal.TryParse(p.Amount, out var a) ? a : 0,
            Currency = p.Currency,
            Status = p.Status,
            StripePaymentIntentId = p.StripePaymentIntentId,
            CreatedAt = p.CreatedAt
        }).ToList();
    }

    // ── Discounts ─────────────────────────────────────

    [Authorize]
    public async Task<List<Coupon>> GetCoupons(
        DiscountGrpc.DiscountGrpcClient client)
    {
        var reply = await client.GetCouponsAsync(new GetCouponsRequest());
        return reply.Coupons.Select(c => MapCoupon(c)).ToList();
    }

    public async Task<DiscountValidation> ValidateDiscount(
        string couponCode,
        decimal orderAmount,
        DiscountGrpc.DiscountGrpcClient client)
    {
        var reply = await client.ValidateDiscountAsync(new ValidateDiscountGrpcRequest
        {
            CouponCode = couponCode,
            OrderAmount = orderAmount.ToString()
        });

        return new DiscountValidation
        {
            IsValid = reply.IsValid,
            Error = reply.Error,
            DiscountAmount = decimal.TryParse(reply.DiscountAmount, out var d) ? d : 0,
            DiscountType = reply.DiscountType,
            CouponCode = reply.CouponCode
        };
    }

    // ── Cart ─────────────────────────────────────────

    [Authorize]
    public async Task<Cart?> GetMyCart(
        ClaimsPrincipal claimsPrincipal,
        CartGrpc.CartGrpcClient client)
    {
        var userId = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return null;

        var reply = await client.GetCartAsync(new GetCartRequest { CartId = userId });
        return new Cart
        {
            Id = reply.Id,
            TotalPrice = decimal.TryParse(reply.TotalPrice, out var t) ? t : 0,
            LastModifiedAt = reply.LastModifiedAt,
            Items = reply.Items.Select(i => new CartItem
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = decimal.TryParse(i.UnitPrice, out var u) ? u : 0,
                LineTotal = decimal.TryParse(i.LineTotal, out var l) ? l : 0
            }).ToList()
        };
    }

    // ── Helpers ──────────────────────────────────────

    private static Coupon MapCoupon(CouponReply c) => new()
    {
        Id = c.Id,
        Code = c.Code,
        DiscountType = c.DiscountType,
        Value = decimal.TryParse(c.Value, out var v) ? v : 0,
        MinOrderAmount = decimal.TryParse(c.MinOrderAmount, out var m) ? m : 0,
        MaxUses = c.MaxUses,
        CurrentUses = c.CurrentUses,
        ExpiresAt = c.ExpiresAt,
        IsActive = c.IsActive,
        CreatedAt = c.CreatedAt
    };

    private static Category MapCategory(CategoryReply reply) => new()
    {
        Id = reply.Id,
        Name = reply.Name,
        Slug = reply.Slug,
        ParentId = reply.ParentId == 0 ? null : reply.ParentId,
        Children = reply.Children.Select(MapCategory).ToList()
    };

    private static Order MapOrder(OrderReply reply) => new()
    {
        OrderId = reply.OrderId,
        CustomerId = reply.CustomerId,
        Status = reply.Status,
        TotalAmount = decimal.TryParse(reply.TotalAmount, out var a) ? a : 0,
        ItemsJson = reply.ItemsJson,
        CreatedAt = reply.CreatedAt,
        UpdatedAt = reply.UpdatedAt
    };
}

public class ProductConnection
{
    public List<Product> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
