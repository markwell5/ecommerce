using System.Text.Json;
using Ecommerce.Events.Order.Messages;
using PactNet;
using PactNet.Verifier;
using Xunit.Abstractions;

namespace Ecommerce.Contract.Tests.Providers;

public class OrderServiceProviderTests
{
    private readonly ITestOutputHelper _output;

    public OrderServiceProviderTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void OrderService_HonorsContractWith_StockService()
    {
        var pactPath = Path.Combine("..", "..", "..", "..", "pacts", "StockService-OrderService.json");

        if (!File.Exists(pactPath))
        {
            _output.WriteLine("Pact file not found — run consumer tests first");
            return;
        }

        var config = new PactVerifierConfig
        {
            LogLevel = PactLogLevel.Information
        };

        using var verifier = new PactVerifier("OrderService", config);

        verifier
            .WithMessages(scenarios =>
            {
                scenarios.Add("a ReserveStock command", () => new ReserveStock
                {
                    OrderId = Guid.Parse("d290f1ee-6c54-4b01-90e6-d701748f0851"),
                    ItemsJson = "[{\"ProductId\":1,\"Quantity\":2}]"
                });

                scenarios.Add("a ReleaseStock command", () => new ReleaseStock
                {
                    OrderId = Guid.Parse("d290f1ee-6c54-4b01-90e6-d701748f0851"),
                    ItemsJson = "[{\"ProductId\":1,\"Quantity\":2}]"
                });
            }, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            })
            .WithFileSource(new FileInfo(pactPath))
            .Verify();
    }

    [Fact]
    public void OrderService_HonorsContractWith_PaymentService()
    {
        var pactPath = Path.Combine("..", "..", "..", "..", "pacts", "PaymentService-OrderService.json");

        if (!File.Exists(pactPath))
        {
            _output.WriteLine("Pact file not found — run consumer tests first");
            return;
        }

        var config = new PactVerifierConfig
        {
            LogLevel = PactLogLevel.Information
        };

        using var verifier = new PactVerifier("OrderService", config);

        verifier
            .WithMessages(scenarios =>
            {
                scenarios.Add("a ProcessPayment command", () => new ProcessPayment
                {
                    OrderId = Guid.Parse("d290f1ee-6c54-4b01-90e6-d701748f0851"),
                    Amount = 99.99m,
                    CustomerId = "customer-1"
                });

                scenarios.Add("a RefundPayment command", () => new RefundPayment
                {
                    OrderId = Guid.Parse("d290f1ee-6c54-4b01-90e6-d701748f0851")
                });
            }, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            })
            .WithFileSource(new FileInfo(pactPath))
            .Verify();
    }
}
