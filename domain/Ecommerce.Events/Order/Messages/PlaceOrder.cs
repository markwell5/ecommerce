using System;

namespace Ecommerce.Events.Order.Messages
{
    public record PlaceOrder
    {
        public Guid OrderId { get; init; }
        public string CustomerId { get; init; }
        public decimal TotalAmount { get; init; }
        public string ItemsJson { get; init; }
    }
}
