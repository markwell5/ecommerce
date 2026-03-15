using System.Text.Json;
using Ecommerce.Events.Order.Messages;
using PactNet;
using PactNet.Verifier;
using Xunit.Abstractions;

namespace Ecommerce.Contract.Tests.Providers;

public class StockServiceProviderTests
{
    private readonly ITestOutputHelper _output;

    public StockServiceProviderTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void StockService_HonorsContractWith_OrderService()
    {
        var pactPath = Path.Combine("..", "..", "..", "..", "pacts", "OrderService-StockService.json");

        if (!File.Exists(pactPath))
        {
            _output.WriteLine("Pact file not found — run consumer tests first");
            return;
        }

        var config = new PactVerifierConfig
        {
            LogLevel = PactLogLevel.Information
        };

        using var verifier = new PactVerifier("StockService", config);

        verifier
            .WithMessages(scenarios =>
            {
                scenarios.Add("a StockReserved event", () => new StockReserved
                {
                    OrderId = Guid.Parse("d290f1ee-6c54-4b01-90e6-d701748f0851")
                });

                scenarios.Add("a StockReservationFailed event", () => new StockReservationFailed
                {
                    OrderId = Guid.Parse("d290f1ee-6c54-4b01-90e6-d701748f0851"),
                    Reason = "Insufficient stock for product 1"
                });
            }, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            })
            .WithFileSource(new FileInfo(pactPath))
            .Verify();
    }
}
