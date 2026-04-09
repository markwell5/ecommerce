using System;

namespace Subscription.Application.Entities
{
    public class Subscription
    {
        public long Id { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public long ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Frequency { get; set; } = "monthly"; // weekly, fortnightly, monthly, custom
        public int IntervalDays { get; set; } = 30;
        public decimal DiscountPercent { get; set; }
        public string Status { get; set; } = "Active"; // Active, Paused, Cancelled
        public long DeliveryAddressId { get; set; }
        public DateTime NextRenewalAt { get; set; }
        public DateTime? LastRenewedAt { get; set; }
        public int FailureCount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
