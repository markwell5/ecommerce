using System;
using System.Collections.Generic;

namespace Wishlist.Application.Entities
{
    public class Wishlist
    {
        public long Id { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public Guid ShareToken { get; set; } = Guid.NewGuid();
        public bool IsPublic { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public List<WishlistItem> Items { get; set; } = new();
    }
}
