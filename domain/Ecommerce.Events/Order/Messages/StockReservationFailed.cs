using System;

namespace Ecommerce.Events.Order.Messages
{
    public record StockReservationFailed
    {
        public Guid OrderId { get; init; }
        public string Reason { get; init; }
    }
}
