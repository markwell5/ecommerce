using System;

namespace Ecommerce.Events.Order
{
    public class OrderCancelled : EventBase
    {
        public Guid OrderId { get; init; }
    }
}
