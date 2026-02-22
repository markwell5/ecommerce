using System;

namespace Ecommerce.Events.Order
{
    public class OrderPaymentFailed : EventBase
    {
        public Guid OrderId { get; init; }
        public string Reason { get; init; }
    }
}
