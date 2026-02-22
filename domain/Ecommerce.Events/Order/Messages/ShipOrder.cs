using System;

namespace Ecommerce.Events.Order.Messages
{
    public record ShipOrder
    {
        public Guid OrderId { get; init; }
    }
}
