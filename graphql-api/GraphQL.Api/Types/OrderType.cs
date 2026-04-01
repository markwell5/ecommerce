using System.Text.Json;
using GraphQL.Api.DataLoaders;

namespace GraphQL.Api.Types;

public class Order
{
    public string OrderId { get; set; } = default!;
    public string CustomerId { get; set; } = default!;
    public string Status { get; set; } = default!;
    public decimal TotalAmount { get; set; }
    public string CreatedAt { get; set; } = default!;
    public string UpdatedAt { get; set; } = default!;

    [GraphQLIgnore]
    public string ItemsJson { get; set; } = default!;
}

public class OrderItem
{
    public long ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

[ExtendObjectType(typeof(Order))]
public class OrderTypeExtensions
{
    public List<OrderItem> GetItems([Parent] Order order)
    {
        if (string.IsNullOrEmpty(order.ItemsJson))
            return [];

        return JsonSerializer.Deserialize<List<OrderItem>>(order.ItemsJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
    }

    public async Task<Payment?> GetPayment(
        [Parent] Order order,
        PaymentByOrderDataLoader loader)
    {
        return await loader.LoadAsync(order.OrderId);
    }

    public async Task<User?> GetCustomer(
        [Parent] Order order,
        UserDataLoader loader)
    {
        return await loader.LoadAsync(order.CustomerId);
    }
}
