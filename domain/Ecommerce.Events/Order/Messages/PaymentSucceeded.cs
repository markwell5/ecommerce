using System;

namespace Ecommerce.Events.Order.Messages
{
    public record PaymentSucceeded
    {
        public Guid OrderId { get; init; }
    }
}
