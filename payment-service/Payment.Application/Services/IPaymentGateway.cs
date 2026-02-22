using System.Collections.Generic;
using System.Threading.Tasks;

namespace Payment.Application.Services
{
    public class PaymentIntentResult
    {
        public string PaymentIntentId { get; set; }
        public string Status { get; set; }
    }

    public class RefundResult
    {
        public string RefundId { get; set; }
        public string Status { get; set; }
    }

    public interface IPaymentGateway
    {
        Task<PaymentIntentResult> CreatePaymentIntentAsync(decimal amount, string currency, Dictionary<string, string> metadata);
        Task<RefundResult> CreateRefundAsync(string paymentIntentId, decimal amount);
    }
}
