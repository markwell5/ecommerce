using System;

namespace Analytics.Application.Entities
{
    public class AnalyticsOrder
    {
        public Guid OrderId { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public string Status { get; set; } = "Placed";
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal RefundAmount { get; set; }
        public DateTime PlacedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime? ReturnedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
