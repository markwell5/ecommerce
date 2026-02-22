using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ecommerce.Events.Order.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;
using Payment.Application.Services;

namespace Payment.Application.Consumers
{
    public class ProcessPaymentConsumer : IConsumer<ProcessPayment>
    {
        private readonly PaymentDbContext _dbContext;
        private readonly IPaymentGateway _paymentGateway;
        private readonly ILogger<ProcessPaymentConsumer> _logger;

        public ProcessPaymentConsumer(PaymentDbContext dbContext, IPaymentGateway paymentGateway, ILogger<ProcessPaymentConsumer> logger)
        {
            _dbContext = dbContext;
            _paymentGateway = paymentGateway;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ProcessPayment> context)
        {
            var orderId = context.Message.OrderId;
            var amount = context.Message.Amount;
            var customerId = context.Message.CustomerId;

            _logger.LogInformation("Processing payment of {Amount} for order {OrderId}", amount, orderId);

            var payment = new Entities.Payment
            {
                OrderId = orderId,
                CustomerId = customerId,
                Amount = amount,
                Currency = "usd",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Payments.Add(payment);
            await _dbContext.SaveChangesAsync();

            try
            {
                var result = await _paymentGateway.CreatePaymentIntentAsync(amount, "usd", new Dictionary<string, string>
                {
                    ["orderId"] = orderId.ToString(),
                    ["customerId"] = customerId
                });

                payment.StripePaymentIntentId = result.PaymentIntentId;
                payment.Status = "Succeeded";
                payment.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Payment succeeded for order {OrderId}, PaymentIntent {PaymentIntentId}",
                    orderId, result.PaymentIntentId);

                await context.Publish(new PaymentSucceeded { OrderId = orderId });
            }
            catch (Exception ex)
            {
                payment.Status = "Failed";
                payment.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();

                _logger.LogWarning(ex, "Payment failed for order {OrderId}: {Reason}", orderId, ex.Message);

                await context.Publish(new PaymentFailed
                {
                    OrderId = orderId,
                    Reason = ex.Message
                });
            }
        }
    }
}
