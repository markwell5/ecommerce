using System;

namespace Ecommerce.Events.Order
{
    public class OrderShipped : EventBase
    {
        public Guid OrderId { get; init; }
    }
}
