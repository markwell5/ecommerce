using System;

namespace Ecommerce.Events.Order.Messages
{
    public record StockReserved
    {
        public Guid OrderId { get; init; }
    }
}
