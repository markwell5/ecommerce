using System;

namespace Ecommerce.Events.Subscription
{
    public class SubscriptionCreated : EventBase
    {
        public long SubscriptionId { get; init; }
        public string CustomerId { get; init; } = string.Empty;
        public long ProductId { get; init; }
        public string Frequency { get; init; } = string.Empty;
    }
}
