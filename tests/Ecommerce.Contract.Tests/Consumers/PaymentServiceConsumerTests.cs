using System.Text.Json;
using Ecommerce.Events.Order.Messages;
using FluentAssertions;
using PactNet;
using Xunit.Abstractions;

namespace Ecommerce.Contract.Tests.Consumers;

public class PaymentServiceConsumerTests
{
    private readonly IMessagePactBuilderV4 _pactBuilder;

    public PaymentServiceConsumerTests(ITestOutputHelper output)
    {
        var pact = Pact.V4("PaymentService", "OrderService", new PactConfig
        {
            PactDir = Path.Combine("..", "..", "..", "..", "pacts"),
            DefaultJsonSettings = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }
        });

        _pactBuilder = pact.WithMessageInteractions();
    }

    [Fact]
    public void PaymentService_CanConsume_ProcessPayment()
    {
        _pactBuilder
            .ExpectsToReceive("a ProcessPayment command")
            .WithJsonContent(new
            {
                orderId = "d290f1ee-6c54-4b01-90e6-d701748f0851",
                amount = 99.99m,
                customerId = "customer-1"
            })
            .Verify<ProcessPayment>(message =>
            {
                message.OrderId.Should().NotBeEmpty();
                message.Amount.Should().BeGreaterThan(0);
                message.CustomerId.Should().NotBeNullOrEmpty();
            });
    }

    [Fact]
    public void PaymentService_CanConsume_RefundPayment()
    {
        _pactBuilder
            .ExpectsToReceive("a RefundPayment command")
            .WithJsonContent(new
            {
                orderId = "d290f1ee-6c54-4b01-90e6-d701748f0851"
            })
            .Verify<RefundPayment>(message =>
            {
                message.OrderId.Should().NotBeEmpty();
            });
    }
}
