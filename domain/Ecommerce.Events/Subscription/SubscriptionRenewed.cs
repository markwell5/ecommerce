using System;

namespace Ecommerce.Events.Subscription
{
    public class SubscriptionRenewed : EventBase
    {
        public long SubscriptionId { get; init; }
        public string CustomerId { get; init; } = string.Empty;
        public string OrderId { get; init; } = string.Empty;
    }
}
