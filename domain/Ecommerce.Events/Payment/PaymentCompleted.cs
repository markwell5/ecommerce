using System;

namespace Ecommerce.Events.Payment
{
    public class PaymentCompleted : EventBase
    {
        public Guid OrderId { get; init; }
        public string CustomerId { get; init; } = string.Empty;
        public decimal Amount { get; init; }
    }
}
