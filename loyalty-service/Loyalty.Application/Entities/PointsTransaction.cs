using System;

namespace Loyalty.Application.Entities
{
    public class PointsTransaction
    {
        public long Id { get; set; }
        public long LoyaltyAccountId { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // earn, redeem, expire, bonus, adjust
        public int Points { get; set; }
        public int BalanceAfter { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? OrderId { get; set; }
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMonths(12);
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
