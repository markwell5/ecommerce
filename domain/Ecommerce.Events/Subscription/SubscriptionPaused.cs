using System;

namespace Ecommerce.Events.Subscription
{
    public class SubscriptionPaused : EventBase
    {
        public long SubscriptionId { get; init; }
        public string CustomerId { get; init; } = string.Empty;
    }
}
