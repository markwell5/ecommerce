using System;

namespace Ecommerce.Model.Subscription.Response
{
    public class SubscriptionResponse
    {
        public long Id { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public long ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Frequency { get; set; } = string.Empty;
        public int IntervalDays { get; set; }
        public decimal DiscountPercent { get; set; }
        public string Status { get; set; } = string.Empty;
        public long DeliveryAddressId { get; set; }
        public DateTime NextRenewalAt { get; set; }
        public DateTime? LastRenewedAt { get; set; }
        public int FailureCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
