using System;

namespace Ecommerce.Events.Order.Messages
{
    public record ProcessPayment
    {
        public Guid OrderId { get; init; }
        public decimal Amount { get; init; }
        public string CustomerId { get; init; }
    }
}
