using System;
using System.Collections.Generic;

namespace Ecommerce.Model.Wishlist.Response
{
    public class WishlistResponse
    {
        public long Id { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public Guid ShareToken { get; set; }
        public bool IsPublic { get; set; }
        public List<WishlistItemResponse> Items { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
