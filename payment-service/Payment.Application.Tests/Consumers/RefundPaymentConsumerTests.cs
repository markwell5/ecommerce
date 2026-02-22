using System;
using System.Threading.Tasks;
using Ecommerce.Events.Order.Messages;
using Ecommerce.Events.Payment;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Payment.Application;
using Payment.Application.Consumers;
using Payment.Application.Services;

namespace Payment.Application.Tests.Consumers;

public class RefundPaymentConsumerTests
{
    private readonly PaymentDbContext _dbContext;
    private readonly IPaymentGateway _paymentGateway;
    private readonly RefundPaymentConsumer _consumer;

    public RefundPaymentConsumerTests()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new PaymentDbContext(options);

        _paymentGateway = Substitute.For<IPaymentGateway>();
        var logger = Substitute.For<ILogger<RefundPaymentConsumer>>();

        _consumer = new RefundPaymentConsumer(_dbContext, _paymentGateway, logger);
    }

    [Fact]
    public async Task Consume_SucceededPayment_ShouldRefundAndPublishEvent()
    {
        var orderId = Guid.NewGuid();
        _dbContext.Payments.Add(new Entities.Payment
        {
            OrderId = orderId,
            CustomerId = "cust-1",
            Amount = 50.00m,
            Status = "Succeeded",
            StripePaymentIntentId = "pi_456"
        });
        await _dbContext.SaveChangesAsync();

        _paymentGateway.CreateRefundAsync("pi_456", 50.00m)
            .Returns(new RefundResult { RefundId = "re_789", Status = "succeeded" });

        var context = Substitute.For<ConsumeContext<RefundPayment>>();
        context.Message.Returns(new RefundPayment { OrderId = orderId });

        await _consumer.Consume(context);

        await context.Received(1).Publish(Arg.Is<PaymentRefunded>(e =>
            e.OrderId == orderId && e.Amount == 50.00m));

        var payment = await _dbContext.Payments.FirstAsync();
        payment.Status.Should().Be("Refunded");

        var refund = await _dbContext.Refunds.FirstAsync();
        refund.StripeRefundId.Should().Be("re_789");
        refund.Amount.Should().Be(50.00m);
    }

    [Fact]
    public async Task Consume_NoPaymentFound_ShouldSkip()
    {
        var context = Substitute.For<ConsumeContext<RefundPayment>>();
        context.Message.Returns(new RefundPayment { OrderId = Guid.NewGuid() });

        await _consumer.Consume(context);

        await context.DidNotReceive().Publish(Arg.Any<PaymentRefunded>());
        await _paymentGateway.DidNotReceive().CreateRefundAsync(Arg.Any<string>(), Arg.Any<decimal>());
    }
}
