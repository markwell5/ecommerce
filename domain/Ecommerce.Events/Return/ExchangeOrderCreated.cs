using System;

namespace Ecommerce.Events.Return
{
    public class ExchangeOrderCreated
    {
        public long ReturnRequestId { get; init; }
        public Guid OriginalOrderId { get; init; }
        public Guid ExchangeOrderId { get; init; }
        public string CustomerId { get; init; } = string.Empty;
        public long ExchangeProductId { get; init; }
        public int Quantity { get; init; }
    }
}
