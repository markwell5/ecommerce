using System.Text.Json;
using Ecommerce.Events.Order.Messages;
using FluentAssertions;
using PactNet;
using Xunit.Abstractions;

namespace Ecommerce.Contract.Tests.Consumers;

public class OrderServiceConsumerTests
{
    private readonly IMessagePactBuilderV4 _pactBuilderFromStock;
    private readonly IMessagePactBuilderV4 _pactBuilderFromPayment;

    public OrderServiceConsumerTests(ITestOutputHelper output)
    {
        var pactConfig = new PactConfig
        {
            PactDir = Path.Combine("..", "..", "..", "..", "pacts"),
            DefaultJsonSettings = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }
        };

        _pactBuilderFromStock = Pact.V4("OrderService", "StockService", pactConfig)
            .WithMessageInteractions();

        _pactBuilderFromPayment = Pact.V4("OrderService", "PaymentService", pactConfig)
            .WithMessageInteractions();
    }

    [Fact]
    public void OrderService_CanConsume_StockReserved()
    {
        _pactBuilderFromStock
            .ExpectsToReceive("a StockReserved event")
            .WithJsonContent(new
            {
                orderId = "d290f1ee-6c54-4b01-90e6-d701748f0851"
            })
            .Verify<StockReserved>(message =>
            {
                message.OrderId.Should().NotBeEmpty();
            });
    }

    [Fact]
    public void OrderService_CanConsume_StockReservationFailed()
    {
        _pactBuilderFromStock
            .ExpectsToReceive("a StockReservationFailed event")
            .WithJsonContent(new
            {
                orderId = "d290f1ee-6c54-4b01-90e6-d701748f0851",
                reason = "Insufficient stock for product 1"
            })
            .Verify<StockReservationFailed>(message =>
            {
                message.OrderId.Should().NotBeEmpty();
                message.Reason.Should().NotBeNullOrEmpty();
            });
    }

    [Fact]
    public void OrderService_CanConsume_PaymentSucceeded()
    {
        _pactBuilderFromPayment
            .ExpectsToReceive("a PaymentSucceeded event")
            .WithJsonContent(new
            {
                orderId = "d290f1ee-6c54-4b01-90e6-d701748f0851"
            })
            .Verify<PaymentSucceeded>(message =>
            {
                message.OrderId.Should().NotBeEmpty();
            });
    }

    [Fact]
    public void OrderService_CanConsume_PaymentFailed()
    {
        _pactBuilderFromPayment
            .ExpectsToReceive("a PaymentFailed event")
            .WithJsonContent(new
            {
                orderId = "d290f1ee-6c54-4b01-90e6-d701748f0851",
                reason = "Card declined"
            })
            .Verify<PaymentFailed>(message =>
            {
                message.OrderId.Should().NotBeEmpty();
                message.Reason.Should().NotBeNullOrEmpty();
            });
    }
}
