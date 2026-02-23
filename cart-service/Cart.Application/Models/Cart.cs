namespace Cart.Application.Models;

public class Cart
{
    public string Id { get; set; } = string.Empty;
    public List<CartItem> Items { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastModifiedAt { get; set; } = DateTime.UtcNow;

    public decimal TotalPrice => Items.Sum(i => i.UnitPrice * i.Quantity);
}

public class CartItem
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
