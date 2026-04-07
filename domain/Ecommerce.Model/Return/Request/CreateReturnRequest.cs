using System;

namespace Ecommerce.Model.Return.Request
{
    public class CreateReturnRequest
    {
        public Guid OrderId { get; set; }
        public long ProductId { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class ReviewReturnRequest
    {
        public string AdminNotes { get; set; } = string.Empty;
    }

    public class InspectReturnRequest
    {
        public string InspectionNotes { get; set; } = string.Empty;
    }

    public class ResolveReturnRequest
    {
        public string Resolution { get; set; } = string.Empty; // full_refund, partial_refund, exchange
        public decimal RefundAmount { get; set; }
    }
}
