using System.Security.Claims;
using Ecommerce.Shared.Protos;
using HotChocolate.Authorization;

namespace GraphQL.Api.Types;

[Authorize]
public class Mutation
{
    // ── Products ──────────────────────────────────────

    public async Task<Product> CreateProduct(
        string name,
        string description,
        string category,
        decimal price,
        ProductGrpc.ProductGrpcClient client)
    {
        var reply = await client.CreateProductAsync(new CreateProductGrpcRequest
        {
            Name = name,
            Description = description,
            Category = category,
            Price = price.ToString()
        });

        return MapProduct(reply);
    }

    public async Task<Product> UpdateProduct(
        long id,
        string name,
        string description,
        string category,
        decimal price,
        ProductGrpc.ProductGrpcClient client)
    {
        var reply = await client.UpdateProductAsync(new UpdateProductGrpcRequest
        {
            Id = id,
            Name = name,
            Description = description,
            Category = category,
            Price = price.ToString()
        });

        return MapProduct(reply);
    }

    public async Task<bool> DeleteProduct(
        long id,
        ProductGrpc.ProductGrpcClient client)
    {
        var reply = await client.DeleteProductAsync(new DeleteProductGrpcRequest { Id = id });
        return reply.Success;
    }

    // ── Categories ────────────────────────────────────

    public async Task<Category> CreateCategory(
        string name,
        string slug,
        long? parentId,
        CategoryGrpc.CategoryGrpcClient client)
    {
        var reply = await client.CreateCategoryAsync(new CreateCategoryGrpcRequest
        {
            Name = name,
            Slug = slug,
            ParentId = parentId ?? 0
        });

        return MapCategory(reply);
    }

    public async Task<Category> UpdateCategory(
        long id,
        string name,
        string slug,
        long? parentId,
        CategoryGrpc.CategoryGrpcClient client)
    {
        var reply = await client.UpdateCategoryAsync(new UpdateCategoryGrpcRequest
        {
            Id = id,
            Name = name,
            Slug = slug,
            ParentId = parentId ?? 0
        });

        return MapCategory(reply);
    }

    public async Task<bool> DeleteCategory(
        long id,
        CategoryGrpc.CategoryGrpcClient client)
    {
        var reply = await client.DeleteCategoryAsync(new DeleteCategoryGrpcRequest { Id = id });
        return reply.Success;
    }

    public async Task<bool> AssignProductCategory(
        long productId,
        long categoryId,
        CategoryGrpc.CategoryGrpcClient client)
    {
        var reply = await client.AssignProductCategoryAsync(new ProductCategoryGrpcRequest
        {
            ProductId = productId,
            CategoryId = categoryId
        });
        return reply.Success;
    }

    public async Task<bool> RemoveProductCategory(
        long productId,
        long categoryId,
        CategoryGrpc.CategoryGrpcClient client)
    {
        var reply = await client.RemoveProductCategoryAsync(new ProductCategoryGrpcRequest
        {
            ProductId = productId,
            CategoryId = categoryId
        });
        return reply.Success;
    }

    // ── Discounts ─────────────────────────────────────

    public async Task<Coupon> CreateCoupon(
        string code,
        string discountType,
        decimal value,
        decimal minOrderAmount,
        int maxUses,
        string expiresAt,
        DiscountGrpc.DiscountGrpcClient client)
    {
        var reply = await client.CreateCouponAsync(new CreateCouponGrpcRequest
        {
            Code = code,
            DiscountType = discountType,
            Value = value.ToString(),
            MinOrderAmount = minOrderAmount.ToString(),
            MaxUses = maxUses,
            ExpiresAt = expiresAt
        });

        return MapCoupon(reply);
    }

    public async Task<Coupon> UpdateCoupon(
        long id,
        string discountType,
        decimal value,
        decimal minOrderAmount,
        int maxUses,
        string expiresAt,
        bool isActive,
        DiscountGrpc.DiscountGrpcClient client)
    {
        var reply = await client.UpdateCouponAsync(new UpdateCouponGrpcRequest
        {
            Id = id,
            DiscountType = discountType,
            Value = value.ToString(),
            MinOrderAmount = minOrderAmount.ToString(),
            MaxUses = maxUses,
            ExpiresAt = expiresAt,
            IsActive = isActive
        });

        return MapCoupon(reply);
    }

    // ── Orders ───────────────────────────────────────

    public async Task<Order> PlaceOrder(
        List<OrderItemInput> items,
        ClaimsPrincipal claimsPrincipal,
        OrderGrpc.OrderGrpcClient client)
    {
        var customerId = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        var request = new PlaceOrderGrpcRequest { CustomerId = customerId };
        foreach (var item in items)
        {
            request.Items.Add(new OrderLineItemGrpc
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice.ToString()
            });
        }

        var reply = await client.PlaceOrderAsync(request);
        return MapOrder(reply);
    }

    public async Task<bool> CancelOrder(
        string orderId,
        OrderGrpc.OrderGrpcClient client)
    {
        var reply = await client.CancelOrderAsync(new OrderActionRequest { OrderId = orderId });
        return reply.Success;
    }

    public async Task<bool> ShipOrder(
        string orderId,
        OrderGrpc.OrderGrpcClient client)
    {
        var reply = await client.ShipOrderAsync(new OrderActionRequest { OrderId = orderId });
        return reply.Success;
    }

    public async Task<bool> DeliverOrder(
        string orderId,
        OrderGrpc.OrderGrpcClient client)
    {
        var reply = await client.DeliverOrderAsync(new OrderActionRequest { OrderId = orderId });
        return reply.Success;
    }

    public async Task<bool> ReturnOrder(
        string orderId,
        OrderGrpc.OrderGrpcClient client)
    {
        var reply = await client.ReturnOrderAsync(new OrderActionRequest { OrderId = orderId });
        return reply.Success;
    }

    // ── Returns ─────────────────────────────────────

    public async Task<ReturnRequest> CreateReturnRequest(
        string orderId,
        long productId,
        int quantity,
        string reason,
        string deliveredAt,
        ClaimsPrincipal claimsPrincipal,
        ReturnsGrpc.ReturnsGrpcClient client)
    {
        var customerId = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        var reply = await client.CreateReturnAsync(new CreateReturnGrpcRequest
        {
            OrderId = orderId,
            CustomerId = customerId,
            ProductId = productId,
            Quantity = quantity,
            Reason = reason,
            DeliveredAt = deliveredAt
        });

        return MapReturn(reply);
    }

    public async Task<ReturnRequest> ApproveReturn(
        long id,
        string adminNotes,
        ReturnsGrpc.ReturnsGrpcClient client)
    {
        var reply = await client.ApproveReturnAsync(new ApproveReturnGrpcRequest { Id = id, AdminNotes = adminNotes });
        return MapReturn(reply);
    }

    public async Task<ReturnRequest> RejectReturn(
        long id,
        string adminNotes,
        ReturnsGrpc.ReturnsGrpcClient client)
    {
        var reply = await client.RejectReturnAsync(new RejectReturnGrpcRequest { Id = id, AdminNotes = adminNotes });
        return MapReturn(reply);
    }

    public async Task<ReturnRequest> ResolveReturn(
        long id,
        string resolution,
        decimal refundAmount,
        ReturnsGrpc.ReturnsGrpcClient client)
    {
        var reply = await client.ResolveReturnAsync(new ResolveReturnGrpcRequest
        {
            Id = id,
            Resolution = resolution,
            RefundAmount = refundAmount.ToString(System.Globalization.CultureInfo.InvariantCulture)
        });

        return MapReturn(reply);
    }

    // ── Stock ────────────────────────────────────────

    public async Task<StockLevel> UpdateStock(
        long productId,
        int quantity,
        StockGrpc.StockGrpcClient client)
    {
        var reply = await client.UpdateStockAsync(new UpdateStockGrpcRequest
        {
            ProductId = productId,
            Quantity = quantity
        });

        return new StockLevel
        {
            ProductId = reply.ProductId,
            AvailableQuantity = reply.AvailableQuantity,
            ReservedQuantity = reply.ReservedQuantity
        };
    }

    // ── Reviews ──────────────────────────────────────

    public async Task<Review> CreateReview(
        long productId,
        int rating,
        string title,
        string body,
        ClaimsPrincipal claimsPrincipal,
        ReviewGrpc.ReviewGrpcClient client)
    {
        var customerId = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        var reply = await client.CreateReviewAsync(new CreateReviewGrpcRequest
        {
            CustomerId = customerId,
            ProductId = productId,
            Rating = rating,
            Title = title,
            Body = body
        });

        return new Review
        {
            Id = reply.Id,
            ProductId = reply.ProductId,
            CustomerId = reply.CustomerId,
            Rating = reply.Rating,
            Title = reply.Title,
            Body = reply.Body,
            CreatedAt = reply.CreatedAt
        };
    }

    // ── Cart ─────────────────────────────────────────

    public async Task<Cart> AddToCart(
        string cartId,
        long productId,
        int quantity,
        CartGrpc.CartGrpcClient client)
    {
        var reply = await client.AddToCartAsync(new AddToCartGrpcRequest
        {
            CartId = cartId,
            ProductId = productId,
            Quantity = quantity
        });

        return MapCart(reply);
    }

    public async Task<Cart> UpdateCartItemQuantity(
        string cartId,
        long productId,
        int quantity,
        CartGrpc.CartGrpcClient client)
    {
        var reply = await client.UpdateCartItemQuantityAsync(new UpdateCartItemQuantityGrpcRequest
        {
            CartId = cartId,
            ProductId = productId,
            Quantity = quantity
        });

        return MapCart(reply);
    }

    public async Task<Cart> RemoveFromCart(
        string cartId,
        long productId,
        CartGrpc.CartGrpcClient client)
    {
        var reply = await client.RemoveFromCartAsync(new RemoveFromCartGrpcRequest
        {
            CartId = cartId,
            ProductId = productId
        });

        return MapCart(reply);
    }

    public async Task<bool> ClearCart(
        string cartId,
        CartGrpc.CartGrpcClient client)
    {
        var reply = await client.ClearCartAsync(new ClearCartGrpcRequest { CartId = cartId });
        return reply.Success;
    }

    // ── Helpers ──────────────────────────────────────

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

    private static Product MapProduct(ProductReply reply) => new()
    {
        Id = reply.Id,
        Name = reply.Name,
        Description = reply.Description,
        Category = reply.Category,
        Price = decimal.TryParse(reply.Price, out var p) ? p : 0
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

    private static Cart MapCart(CartReply reply) => new()
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

public class OrderItemInput
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
