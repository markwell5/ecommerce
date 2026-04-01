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
