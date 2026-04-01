using GraphQL.Api.DataLoaders;

namespace GraphQL.Api.Types;

public class Cart
{
    public string Id { get; set; } = default!;
    public List<CartItem> Items { get; set; } = [];
    public decimal TotalPrice { get; set; }
    public string LastModifiedAt { get; set; } = default!;
}

public class CartItem
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

[ExtendObjectType(typeof(CartItem))]
public class CartItemTypeExtensions
{
    public async Task<Product?> GetProduct(
        [Parent] CartItem item,
        ProductBatchDataLoader loader)
    {
        return await loader.LoadAsync(item.ProductId);
    }
}
