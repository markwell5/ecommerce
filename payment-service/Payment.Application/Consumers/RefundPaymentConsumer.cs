using System;
using System.Threading.Tasks;
using Ecommerce.Events.Order.Messages;
using Ecommerce.Events.Payment;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Payment.Application.Services;

namespace Payment.Application.Consumers
{
    public class RefundPaymentConsumer : IConsumer<RefundPayment>
    {
        private readonly PaymentDbContext _dbContext;
        private readonly IPaymentGateway _paymentGateway;
        private readonly ILogger<RefundPaymentConsumer> _logger;

        public RefundPaymentConsumer(PaymentDbContext dbContext, IPaymentGateway paymentGateway, ILogger<RefundPaymentConsumer> logger)
        {
            _dbContext = dbContext;
            _paymentGateway = paymentGateway;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<RefundPayment> context)
        {
            var orderId = context.Message.OrderId;

            _logger.LogInformation("Processing refund for order {OrderId}", orderId);

            var payment = await _dbContext.Payments
                .FirstOrDefaultAsync(p => p.OrderId == orderId && p.Status == "Succeeded");

            if (payment == null)
            {
                _logger.LogWarning("No succeeded payment found for order {OrderId}, skipping refund", orderId);
                return;
            }

            try
            {
                var result = await _paymentGateway.CreateRefundAsync(payment.StripePaymentIntentId, payment.Amount);

                var refund = new Entities.Refund
                {
                    PaymentId = payment.Id,
                    StripeRefundId = result.RefundId,
                    Amount = payment.Amount,
                    Reason = "Order cancelled",
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.Refunds.Add(refund);

                payment.Status = "Refunded";
                payment.UpdatedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Refund succeeded for order {OrderId}, Refund {RefundId}",
                    orderId, result.RefundId);

                await context.Publish(new PaymentRefunded
                {
                    OrderId = orderId,
                    PaymentId = payment.Id,
                    Amount = payment.Amount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Refund failed for order {OrderId}: {Reason}", orderId, ex.Message);
                throw;
            }
        }
    }
}
