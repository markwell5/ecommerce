using System;

namespace Ecommerce.Events.Order
{
    public class OrderReturned : EventBase
    {
        public Guid OrderId { get; init; }
    }
}
