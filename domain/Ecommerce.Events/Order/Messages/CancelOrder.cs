using System;

namespace Ecommerce.Events.Order.Messages
{
    public record CancelOrder
    {
        public Guid OrderId { get; init; }
    }
}
