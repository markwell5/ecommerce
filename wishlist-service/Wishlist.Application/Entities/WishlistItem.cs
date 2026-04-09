using System;

namespace Wishlist.Application.Entities
{
    public class WishlistItem
    {
        public long Id { get; set; }
        public long WishlistId { get; set; }
        public long ProductId { get; set; }
        public bool NotifyOnRestock { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        public Wishlist Wishlist { get; set; } = null!;
    }
}
