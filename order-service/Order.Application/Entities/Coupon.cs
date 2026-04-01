using System;

namespace Order.Application.Entities
{
    public class Coupon
    {
        public long Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string DiscountType { get; set; } = "percentage"; // percentage, fixed
        public decimal Value { get; set; }
        public decimal MinOrderAmount { get; set; }
        public int MaxUses { get; set; }
        public int CurrentUses { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
