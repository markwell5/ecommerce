using System;

namespace Ecommerce.Model.Loyalty.Response
{
    public class LoyaltyAccountResponse
    {
        public long Id { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public int PointsBalance { get; set; }
        public int LifetimePoints { get; set; }
        public decimal AnnualSpend { get; set; }
        public string Tier { get; set; } = string.Empty;
        public double PointsMultiplier { get; set; }
        public DateTime? LastActivityAt { get; set; }
        public DateTime TierExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
