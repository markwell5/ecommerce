using System;

namespace Ecommerce.Events.Order
{
    public class OrderRejected : EventBase
    {
        public Guid OrderId { get; init; }
        public string Reason { get; init; }
    }
}
