using System;

namespace Ecommerce.Model.Payment.Response
{
    public class PaymentResponse
    {
        public long Id { get; set; }
        public Guid OrderId { get; set; }
        public string CustomerId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public string StripePaymentIntentId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
