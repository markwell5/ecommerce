using System;

namespace Ecommerce.Events.Payment
{
    public class PaymentRefunded : EventBase
    {
        public Guid OrderId { get; init; }
        public long PaymentId { get; init; }
        public decimal Amount { get; init; }
    }
}
