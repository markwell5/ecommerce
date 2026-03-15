using System.Text.Json;
using Ecommerce.Events.Order.Messages;
using Ecommerce.Events.Product;
using FluentAssertions;
using PactNet;
using Xunit.Abstractions;

namespace Ecommerce.Contract.Tests.Consumers;

public class StockServiceConsumerTests
{
    private readonly IMessagePactBuilderV4 _pactBuilder;

    public StockServiceConsumerTests(ITestOutputHelper output)
    {
        var pact = Pact.V4("StockService", "OrderService", new PactConfig
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
    public void StockService_CanConsume_ReserveStock()
    {
        _pactBuilder
            .ExpectsToReceive("a ReserveStock command")
            .WithJsonContent(new
            {
                orderId = "d290f1ee-6c54-4b01-90e6-d701748f0851",
                itemsJson = "[{\"ProductId\":1,\"Quantity\":2}]"
            })
            .Verify<ReserveStock>(message =>
            {
                message.OrderId.Should().NotBeEmpty();
                message.ItemsJson.Should().NotBeNullOrEmpty();
            });
    }

    [Fact]
    public void StockService_CanConsume_ReleaseStock()
    {
        _pactBuilder
            .ExpectsToReceive("a ReleaseStock command")
            .WithJsonContent(new
            {
                orderId = "d290f1ee-6c54-4b01-90e6-d701748f0851",
                itemsJson = "[{\"ProductId\":1,\"Quantity\":2}]"
            })
            .Verify<ReleaseStock>(message =>
            {
                message.OrderId.Should().NotBeEmpty();
                message.ItemsJson.Should().NotBeNullOrEmpty();
            });
    }

    [Fact]
    public void StockService_CanConsume_ProductCreated()
    {
        var pact = Pact.V4("StockService", "ProductService", new PactConfig
        {
            PactDir = Path.Combine("..", "..", "..", "..", "pacts"),
            DefaultJsonSettings = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }
        });

        pact.WithMessageInteractions()
            .ExpectsToReceive("a ProductCreated event")
            .WithJsonContent(new
            {
                id = 42L
            })
            .Verify<ProductCreated>(message =>
            {
                message.Id.Should().BeGreaterThan(0);
            });
    }
}
