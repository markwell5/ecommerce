using System;

namespace Ecommerce.Events.Order
{
    public class OrderConfirmed : EventBase
    {
        public Guid OrderId { get; init; }
    }
}
