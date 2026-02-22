using System;

namespace Payment.Application.Entities
{
    public class Payment
    {
        public long Id { get; set; }
        public Guid OrderId { get; set; }
        public string CustomerId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "usd";
        public string StripePaymentIntentId { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
