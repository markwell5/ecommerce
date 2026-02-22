using System;

namespace Payment.Application.Entities
{
    public class Refund
    {
        public long Id { get; set; }
        public long PaymentId { get; set; }
        public string StripeRefundId { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Payment Payment { get; set; }
    }
}
