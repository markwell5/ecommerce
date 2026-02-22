using System;

namespace Ecommerce.Events.Order.Messages
{
    public record ReturnOrder
    {
        public Guid OrderId { get; init; }
    }
}
