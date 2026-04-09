using System;

namespace Subscription.Application.Entities
{
    public class RenewalHistory
    {
        public long Id { get; set; }
        public long SubscriptionId { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public string Status { get; set; } = "Success"; // Success, Failed, Skipped
        public string FailureReason { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Subscription Subscription { get; set; } = null!;
    }
}
