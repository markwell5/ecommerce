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
    public async Task<OrderConnection> GetOrders(
        int page,
        int pageSize,
        string? status,
        OrderGrpc.OrderGrpcClient client)
    {
        var reply = await client.GetOrdersAsync(new GetOrdersRequest
        {
            Page = page,
            PageSize = pageSize,
            Status = status ?? string.Empty
        });

        return new OrderConnection
        {
            Items = reply.Orders.Select(MapOrder).ToList(),
            TotalCount = reply.TotalCount,
            Page = reply.Page,
            PageSize = reply.PageSize
        };
    }

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

    // ── Admin: Customers ─────────────────────────────

    [Authorize]
    public async Task<UserConnection> GetUsers(
        int page,
        int pageSize,
        string? search,
        UserGrpc.UserGrpcClient client)
    {
        var reply = await client.GetUsersAsync(new GetUsersRequest
        {
            Page = page,
            PageSize = pageSize,
            Search = search ?? string.Empty
        });

        return new UserConnection
        {
            Items = reply.Users.Select(u => new User
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Phone = u.Phone,
                Role = u.Role,
                CreatedAt = u.CreatedAt
            }).ToList(),
            TotalCount = reply.TotalCount,
            Page = reply.Page,
            PageSize = reply.PageSize
        };
    }

    [Authorize]
    public async Task<User?> GetUser(
        string userId,
        UserDataLoader loader)
    {
        return await loader.LoadAsync(userId);
    }

    [Authorize]
    public async Task<List<Address>> GetUserAddresses(
        string userId,
        UserGrpc.UserGrpcClient client)
    {
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

    [Authorize]
    public async Task<List<Order>> GetOrdersByCustomer(
        string customerId,
        OrderGrpc.OrderGrpcClient client)
    {
        var reply = await client.GetOrdersByCustomerAsync(
            new GetOrdersByCustomerRequest { CustomerId = customerId });

        return reply.Orders.Select(MapOrder).ToList();
    }

    [Authorize]
    public async Task<List<Payment>> GetPaymentsByCustomer(
        string customerId,
        PaymentGrpc.PaymentGrpcClient client)
    {
        var reply = await client.GetPaymentsByCustomerAsync(
            new GetPaymentsByCustomerRequest { CustomerId = customerId });

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

    // ── Analytics ────────────────────────────────────

    [Authorize]
    public async Task<SalesOverview> GetSalesOverview(
        string? from,
        string? to,
        AnalyticsGrpc.AnalyticsGrpcClient client)
    {
        var reply = await client.GetSalesOverviewAsync(new GetSalesOverviewRequest
        {
            From = from ?? string.Empty,
            To = to ?? string.Empty
        });

        return new SalesOverview
        {
            TotalRevenue = decimal.TryParse(reply.TotalRevenue, out var r) ? r : 0,
            OrderCount = reply.OrderCount,
            AvgOrderValue = decimal.TryParse(reply.AvgOrderValue, out var a) ? a : 0,
            CancelledCount = reply.CancelledCount,
            ReturnedCount = reply.ReturnedCount,
            NewCustomerCount = reply.NewCustomerCount
        };
    }

    [Authorize]
    public async Task<List<StatusBreakdownItem>> GetOrderStatusBreakdown(
        string? from,
        string? to,
        AnalyticsGrpc.AnalyticsGrpcClient client)
    {
        var reply = await client.GetOrderStatusBreakdownAsync(new GetOrderStatusBreakdownRequest
        {
            From = from ?? string.Empty,
            To = to ?? string.Empty
        });

        return reply.Statuses.Select(s => new StatusBreakdownItem
        {
            Status = s.Status,
            Count = s.Count
        }).ToList();
    }

    [Authorize]
    public async Task<List<DailyRevenuePoint>> GetDailyRevenue(
        string? from,
        string? to,
        AnalyticsGrpc.AnalyticsGrpcClient client)
    {
        var reply = await client.GetDailyRevenueAsync(new GetDailyRevenueRequest
        {
            From = from ?? string.Empty,
            To = to ?? string.Empty
        });

        return reply.Points.Select(p => new DailyRevenuePoint
        {
            Date = p.Date,
            Revenue = decimal.TryParse(p.Revenue, out var rv) ? rv : 0,
            OrderCount = p.OrderCount
        }).ToList();
    }

    // ── Returns ──────────────────────────────────────

    [Authorize]
    public async Task<ReturnRequest?> GetReturnRequest(
        long id,
        ReturnsGrpc.ReturnsGrpcClient client)
    {
        var reply = await client.GetReturnAsync(new GetReturnRequest { Id = id });
        return MapReturn(reply);
    }

    [Authorize]
    public async Task<List<ReturnRequest>> GetReturnsByOrder(
        string orderId,
        ReturnsGrpc.ReturnsGrpcClient client)
    {
        var reply = await client.GetReturnsByOrderAsync(new GetReturnsByOrderRequest { OrderId = orderId });
        return reply.Returns.Select(MapReturn).ToList();
    }

    [Authorize]
    public async Task<List<ReturnRequest>> GetReturnsByCustomer(
        string customerId,
        ReturnsGrpc.ReturnsGrpcClient client)
    {
        var reply = await client.GetReturnsByCustomerAsync(new GetReturnsByCustomerRequest { CustomerId = customerId });
        return reply.Returns.Select(MapReturn).ToList();
    }

    // ── Loyalty ──────────────────────────────────────

    [Authorize]
    public async Task<LoyaltyAccount?> GetLoyaltyAccount(
        string customerId,
        LoyaltyGrpc.LoyaltyGrpcClient client)
    {
        var reply = await client.GetLoyaltyAccountAsync(new GetLoyaltyAccountRequest { CustomerId = customerId });
        return MapLoyaltyAccount(reply);
    }

    [Authorize]
    public async Task<LoyaltyAccount?> GetMyLoyaltyAccount(
        ClaimsPrincipal claimsPrincipal,
        LoyaltyGrpc.LoyaltyGrpcClient client)
    {
        var userId = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return null;

        var reply = await client.GetLoyaltyAccountAsync(new GetLoyaltyAccountRequest { CustomerId = userId });
        return MapLoyaltyAccount(reply);
    }

    [Authorize]
    public async Task<PointsHistoryConnection> GetPointsHistory(
        string customerId,
        int page,
        int pageSize,
        LoyaltyGrpc.LoyaltyGrpcClient client)
    {
        var reply = await client.GetPointsHistoryAsync(new GetPointsHistoryRequest
        {
            CustomerId = customerId,
            Page = page,
            PageSize = pageSize
        });

        return new PointsHistoryConnection
        {
            Items = reply.Transactions.Select(MapPointsTransaction).ToList(),
            TotalCount = reply.TotalCount,
            Page = reply.Page,
            PageSize = reply.PageSize
        };
    }

    // ── Helpers ──────────────────────────────────────

    private static LoyaltyAccount MapLoyaltyAccount(LoyaltyAccountReply a) => new()
    {
        Id = a.Id,
        CustomerId = a.CustomerId,
        PointsBalance = a.PointsBalance,
        LifetimePoints = a.LifetimePoints,
        AnnualSpend = decimal.TryParse(a.AnnualSpend, out var s) ? s : 0,
        Tier = a.Tier,
        PointsMultiplier = double.TryParse(a.PointsMultiplier, out var m) ? m : 1.0,
        LastActivityAt = string.IsNullOrEmpty(a.LastActivityAt) ? null : a.LastActivityAt,
        TierExpiresAt = a.TierExpiresAt,
        CreatedAt = a.CreatedAt
    };

    private static PointsTransaction MapPointsTransaction(PointsTransactionReply t) => new()
    {
        Id = t.Id,
        CustomerId = t.CustomerId,
        Type = t.Type,
        Points = t.Points,
        BalanceAfter = t.BalanceAfter,
        Description = t.Description,
        OrderId = string.IsNullOrEmpty(t.OrderId) ? null : t.OrderId,
        CreatedAt = t.CreatedAt
    };

    private static ReturnRequest MapReturn(ReturnReply r) => new()
    {
        Id = r.Id,
        RmaNumber = r.RmaNumber,
        OrderId = r.OrderId,
        CustomerId = r.CustomerId,
        ProductId = r.ProductId,
        Quantity = r.Quantity,
        Reason = r.Reason,
        Status = r.Status,
        Resolution = r.Resolution,
        RefundAmount = decimal.TryParse(r.RefundAmount, out var ra) ? ra : 0,
        RestockingFee = decimal.TryParse(r.RestockingFee, out var rf) ? rf : 0,
        InspectionNotes = r.InspectionNotes,
        AdminNotes = r.AdminNotes,
        AutoApproved = r.AutoApproved,
        CreatedAt = r.CreatedAt,
        ApprovedAt = string.IsNullOrEmpty(r.ApprovedAt) ? null : r.ApprovedAt,
        ReceivedAt = string.IsNullOrEmpty(r.ReceivedAt) ? null : r.ReceivedAt,
        ResolvedAt = string.IsNullOrEmpty(r.ResolvedAt) ? null : r.ResolvedAt
    };

    // ── Audit Log ────────────────────────────────────

    [Authorize]
    public async Task<AuditConnection> SearchAuditLog(
        int page,
        int pageSize,
        string? actorId,
        string? entityType,
        string? entityId,
        string? service,
        string? action,
        string? correlationId,
        string? from,
        string? to,
        AuditGrpc.AuditGrpcClient client)
    {
        var reply = await client.SearchAuditEntriesAsync(new SearchAuditEntriesRequest
        {
            Page = page,
            PageSize = pageSize,
            ActorId = actorId ?? string.Empty,
            EntityType = entityType ?? string.Empty,
            EntityId = entityId ?? string.Empty,
            Service = service ?? string.Empty,
            Action = action ?? string.Empty,
            CorrelationId = correlationId ?? string.Empty,
            From = from ?? string.Empty,
            To = to ?? string.Empty
        });

        return new AuditConnection
        {
            Items = reply.Entries.Select(e => new AuditEntryItem
            {
                Id = e.Id,
                Service = e.Service,
                Action = e.Action,
                ActorId = e.ActorId,
                ActorType = e.ActorType,
                EntityType = e.EntityType,
                EntityId = e.EntityId,
                BeforeState = e.BeforeState,
                AfterState = e.AfterState,
                CorrelationId = e.CorrelationId,
                IpAddress = e.IpAddress,
                Hash = e.Hash,
                Timestamp = e.Timestamp
            }).ToList(),
            TotalCount = reply.TotalCount,
            Page = reply.Page,
            PageSize = reply.PageSize
        };
    }

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

public class OrderConnection
{
    public List<Order> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class UserConnection
{
    public List<User> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
