using System;

namespace Ecommerce.Events.Order.Messages
{
    public record DeliverOrder
    {
        public Guid OrderId { get; init; }
    }
}
