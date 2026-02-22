using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ecommerce.Events.Order.Messages;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Payment.Application;
using Payment.Application.Consumers;
using Payment.Application.Services;

namespace Payment.Application.Tests.Consumers;

public class ProcessPaymentConsumerTests
{
    private readonly PaymentDbContext _dbContext;
    private readonly IPaymentGateway _paymentGateway;
    private readonly ProcessPaymentConsumer _consumer;

    public ProcessPaymentConsumerTests()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new PaymentDbContext(options);

        _paymentGateway = Substitute.For<IPaymentGateway>();
        var logger = Substitute.For<ILogger<ProcessPaymentConsumer>>();

        _consumer = new ProcessPaymentConsumer(_dbContext, _paymentGateway, logger);
    }

    [Fact]
    public async Task Consume_SuccessfulPayment_ShouldPublishPaymentSucceeded()
    {
        var orderId = Guid.NewGuid();
        var message = new ProcessPayment
        {
            OrderId = orderId,
            Amount = 99.99m,
            CustomerId = "cust-1"
        };

        _paymentGateway.CreatePaymentIntentAsync(Arg.Any<decimal>(), Arg.Any<string>(), Arg.Any<Dictionary<string, string>>())
            .Returns(new PaymentIntentResult { PaymentIntentId = "pi_123", Status = "succeeded" });

        var context = Substitute.For<ConsumeContext<ProcessPayment>>();
        context.Message.Returns(message);

        await _consumer.Consume(context);

        await context.Received(1).Publish(Arg.Is<PaymentSucceeded>(e => e.OrderId == orderId));
        await context.DidNotReceive().Publish(Arg.Any<PaymentFailed>());

        var payment = await _dbContext.Payments.FirstAsync();
        payment.Status.Should().Be("Succeeded");
        payment.StripePaymentIntentId.Should().Be("pi_123");
    }

    [Fact]
    public async Task Consume_FailedPayment_ShouldPublishPaymentFailed()
    {
        var orderId = Guid.NewGuid();
        var message = new ProcessPayment
        {
            OrderId = orderId,
            Amount = 99.99m,
            CustomerId = "cust-1"
        };

        _paymentGateway.CreatePaymentIntentAsync(Arg.Any<decimal>(), Arg.Any<string>(), Arg.Any<Dictionary<string, string>>())
            .ThrowsAsync(new Exception("Card declined"));

        var context = Substitute.For<ConsumeContext<ProcessPayment>>();
        context.Message.Returns(message);

        await _consumer.Consume(context);

        await context.Received(1).Publish(Arg.Is<PaymentFailed>(e => e.OrderId == orderId && e.Reason == "Card declined"));
        await context.DidNotReceive().Publish(Arg.Any<PaymentSucceeded>());

        var payment = await _dbContext.Payments.FirstAsync();
        payment.Status.Should().Be("Failed");
    }
}
