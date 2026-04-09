using System;

namespace Ecommerce.Model.Return.Response
{
    public class ReturnShipmentResponse
    {
        public long Id { get; set; }
        public long ReturnRequestId { get; set; }
        public string Carrier { get; set; } = string.Empty;
        public string TrackingNumber { get; set; } = string.Empty;
        public string LabelUrl { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string DropOffLocation { get; set; } = string.Empty;
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
