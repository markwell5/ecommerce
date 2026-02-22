using System;

namespace Ecommerce.Events.Order
{
    public class OrderPlaced : EventBase
    {
        public Guid OrderId { get; init; }
        public string CustomerId { get; init; }
        public decimal TotalAmount { get; init; }
    }
}
