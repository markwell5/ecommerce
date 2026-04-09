using System;

namespace Ecommerce.Model.Wishlist.Response
{
    public class WishlistItemResponse
    {
        public long Id { get; set; }
        public long WishlistId { get; set; }
        public long ProductId { get; set; }
        public bool NotifyOnRestock { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
