using System;

namespace Ecommerce.Events.Order
{
    public class OrderDelivered : EventBase
    {
        public Guid OrderId { get; init; }
    }
}
