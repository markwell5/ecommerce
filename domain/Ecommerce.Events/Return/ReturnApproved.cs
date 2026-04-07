using System;

namespace Ecommerce.Events.Return
{
    public class ReturnApproved
    {
        public long ReturnRequestId { get; init; }
        public Guid OrderId { get; init; }
        public long ProductId { get; init; }
        public int Quantity { get; init; }
    }
}
