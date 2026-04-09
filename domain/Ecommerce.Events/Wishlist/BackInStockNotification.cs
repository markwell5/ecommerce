namespace Ecommerce.Events.Wishlist
{
    public class BackInStockNotification : EventBase
    {
        public string CustomerId { get; init; } = string.Empty;
        public long ProductId { get; init; }
        public long WishlistId { get; init; }
        public long WishlistItemId { get; init; }
    }
}
