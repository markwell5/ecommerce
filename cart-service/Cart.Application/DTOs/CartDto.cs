namespace Cart.Application.DTOs;

public class CartDto
{
    public string Id { get; set; } = string.Empty;
    public List<CartItemDto> Items { get; set; } = [];
    public decimal TotalPrice { get; set; }
    public DateTime LastModifiedAt { get; set; }
}
