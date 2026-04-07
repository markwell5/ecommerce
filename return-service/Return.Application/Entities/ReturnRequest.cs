using System;

namespace Return.Application.Entities
{
    public class ReturnRequest
    {
        public long Id { get; set; }
        public string RmaNumber { get; set; } = string.Empty;
        public Guid OrderId { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public long ProductId { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; } = string.Empty; // defective, wrong_item, changed_mind, other
        public string Status { get; set; } = "Requested"; // Requested, Approved, Rejected, Received, Inspected, Refunded, Exchanged
        public string Resolution { get; set; } = string.Empty; // full_refund, partial_refund, exchange
        public decimal RefundAmount { get; set; }
        public decimal RestockingFee { get; set; }
        public string InspectionNotes { get; set; } = string.Empty;
        public string AdminNotes { get; set; } = string.Empty;
        public bool AutoApproved { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovedAt { get; set; }
        public DateTime? ReceivedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
