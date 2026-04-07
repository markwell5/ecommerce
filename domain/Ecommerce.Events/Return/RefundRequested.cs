using System;

namespace Ecommerce.Events.Return
{
    public class RefundRequested
    {
        public long ReturnRequestId { get; init; }
        public Guid OrderId { get; init; }
        public string CustomerId { get; init; } = string.Empty;
        public decimal Amount { get; init; }
    }
}
