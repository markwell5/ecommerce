using System.Text.Json;
using System.Text.Json.Nodes;
using Ecommerce.Events.Order.Messages;
using FluentAssertions;
using Xunit.Abstractions;

namespace Ecommerce.Contract.Tests.Providers;

public class OrderServiceProviderTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly ITestOutputHelper _output;

    public OrderServiceProviderTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void OrderService_ProducesValidMessage_ReserveStock()
    {
        var message = new ReserveStock
        {
            OrderId = Guid.NewGuid(),
            ItemsJson = "[{\"ProductId\":1,\"Quantity\":2}]"
        };

        var json = JsonSerializer.Serialize(message, JsonOptions);
        var node = JsonNode.Parse(json)!;

        node["orderId"].Should().NotBeNull();
        node["itemsJson"].Should().NotBeNull();

        VerifyAgainstPact("StockService-OrderService.json", "a ReserveStock command", node);
    }

    [Fact]
    public void OrderService_ProducesValidMessage_ReleaseStock()
    {
        var message = new ReleaseStock
        {
            OrderId = Guid.NewGuid(),
            ItemsJson = "[{\"ProductId\":1,\"Quantity\":2}]"
        };

        var json = JsonSerializer.Serialize(message, JsonOptions);
        var node = JsonNode.Parse(json)!;

        node["orderId"].Should().NotBeNull();
        node["itemsJson"].Should().NotBeNull();

        VerifyAgainstPact("StockService-OrderService.json", "a ReleaseStock command", node);
    }

    [Fact]
    public void OrderService_ProducesValidMessage_ProcessPayment()
    {
        var message = new ProcessPayment
        {
            OrderId = Guid.NewGuid(),
            Amount = 99.99m,
            CustomerId = "customer-1"
        };

        var json = JsonSerializer.Serialize(message, JsonOptions);
        var node = JsonNode.Parse(json)!;

        node["orderId"].Should().NotBeNull();
        node["amount"].Should().NotBeNull();
        node["customerId"].Should().NotBeNull();

        VerifyAgainstPact("PaymentService-OrderService.json", "a ProcessPayment command", node);
    }

    [Fact]
    public void OrderService_ProducesValidMessage_RefundPayment()
    {
        var message = new RefundPayment
        {
            OrderId = Guid.NewGuid()
        };

        var json = JsonSerializer.Serialize(message, JsonOptions);
        var node = JsonNode.Parse(json)!;

        node["orderId"].Should().NotBeNull();

        VerifyAgainstPact("PaymentService-OrderService.json", "a RefundPayment command", node);
    }

    private void VerifyAgainstPact(string pactFile, string description, JsonNode producedMessage)
    {
        var pactPath = Path.Combine("..", "..", "..", "..", "pacts", pactFile);
        if (!File.Exists(pactPath))
        {
            _output.WriteLine($"Pact file {pactFile} not found — skipping provider verification");
            return;
        }

        var pactJson = JsonNode.Parse(File.ReadAllText(pactPath))!;
        var interactions = pactJson["interactions"]!.AsArray();
        var interaction = interactions.FirstOrDefault(i =>
            i!["description"]!.GetValue<string>() == description);

        interaction.Should().NotBeNull($"pact should contain interaction '{description}'");

        var expectedContent = interaction!["contents"]!["content"]!.AsObject();

        foreach (var property in expectedContent)
        {
            producedMessage[property.Key].Should().NotBeNull(
                $"produced message should contain field '{property.Key}' expected by consumer");
        }
    }
}
