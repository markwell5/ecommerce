using System;

namespace Ecommerce.Events.Order.Messages
{
    public record RefundPayment
    {
        public Guid OrderId { get; init; }
    }
}
