using System;

namespace Ecommerce.Events.Order.Messages
{
    public record PaymentFailed
    {
        public Guid OrderId { get; init; }
        public string Reason { get; init; }
    }
}
