using System;

namespace Ecommerce.Model.Return.Response
{
    public class ReturnResponse
    {
        public long Id { get; set; }
        public string RmaNumber { get; set; } = string.Empty;
        public Guid OrderId { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public long ProductId { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Resolution { get; set; } = string.Empty;
        public decimal RefundAmount { get; set; }
        public decimal RestockingFee { get; set; }
        public string InspectionNotes { get; set; } = string.Empty;
        public string AdminNotes { get; set; } = string.Empty;
        public bool AutoApproved { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? ReceivedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public long? ExchangeProductId { get; set; }
        public string ExchangeProductName { get; set; } = string.Empty;
        public Guid? ExchangeOrderId { get; set; }
    }
}
