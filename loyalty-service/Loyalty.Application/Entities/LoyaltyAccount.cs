using System;

namespace Loyalty.Application.Entities
{
    public class LoyaltyAccount
    {
        public long Id { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public int PointsBalance { get; set; }
        public int LifetimePoints { get; set; }
        public decimal AnnualSpend { get; set; }
        public string Tier { get; set; } = "Bronze"; // Bronze, Silver, Gold, Platinum
        public double PointsMultiplier { get; set; } = 1.0;
        public DateTime? LastActivityAt { get; set; }
        public DateTime TierExpiresAt { get; set; } = DateTime.UtcNow.AddYears(1);
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
