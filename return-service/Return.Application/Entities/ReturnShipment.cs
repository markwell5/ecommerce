using System;

namespace Return.Application.Entities
{
    public class ReturnShipment
    {
        public long Id { get; set; }
        public long ReturnRequestId { get; set; }
        public string Carrier { get; set; } = string.Empty; // ups, fedex, dhl, royal_mail
        public string TrackingNumber { get; set; } = string.Empty;
        public string LabelUrl { get; set; } = string.Empty;
        public string Status { get; set; } = "LabelGenerated"; // LabelGenerated, InTransit, Delivered, PickupScheduled
        public string DropOffLocation { get; set; } = string.Empty;
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
