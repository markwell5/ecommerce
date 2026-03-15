using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Stripe;

namespace Payment.Application.Services
{
    public class StripePaymentGateway : IPaymentGateway
    {
        private readonly ILogger<StripePaymentGateway> _logger;

        public StripePaymentGateway(ILogger<StripePaymentGateway> logger)
        {
            _logger = logger;
        }

        public virtual async Task<PaymentIntentResult> CreatePaymentIntentAsync(decimal amount, string currency, Dictionary<string, string> metadata)
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100),
                Currency = currency,
                Metadata = metadata,
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                    AllowRedirects = "never"
                },
                Confirm = true
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            _logger.LogInformation("Created Stripe PaymentIntent {PaymentIntentId} with status {Status}",
                paymentIntent.Id, paymentIntent.Status);

            return new PaymentIntentResult
            {
                PaymentIntentId = paymentIntent.Id,
                Status = paymentIntent.Status
            };
        }

        public virtual async Task<RefundResult> CreateRefundAsync(string paymentIntentId, decimal amount)
        {
            var options = new RefundCreateOptions
            {
                PaymentIntent = paymentIntentId,
                Amount = (long)(amount * 100)
            };

            var service = new RefundService();
            var refund = await service.CreateAsync(options);

            _logger.LogInformation("Created Stripe Refund {RefundId} for PaymentIntent {PaymentIntentId}",
                refund.Id, paymentIntentId);

            return new RefundResult
            {
                RefundId = refund.Id,
                Status = refund.Status
            };
        }
    }
}
