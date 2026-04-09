using System;

namespace Ecommerce.Events.Order
{
    public class OrderCompleted : EventBase
    {
        public Guid OrderId { get; init; }
        public string CustomerId { get; init; } = string.Empty;
        public decimal TotalAmount { get; init; }
    }
}
