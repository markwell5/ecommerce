using System.Text.Json;
using Ecommerce.Events.Order.Messages;
using PactNet;
using PactNet.Verifier;
using Xunit.Abstractions;

namespace Ecommerce.Contract.Tests.Providers;

public class PaymentServiceProviderTests
{
    private readonly ITestOutputHelper _output;

    public PaymentServiceProviderTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void PaymentService_HonorsContractWith_OrderService()
    {
        var pactPath = Path.Combine("..", "..", "..", "..", "pacts", "OrderService-PaymentService.json");

        if (!File.Exists(pactPath))
        {
            _output.WriteLine("Pact file not found — run consumer tests first");
            return;
        }

        var config = new PactVerifierConfig
        {
            LogLevel = PactLogLevel.Information
        };

        using var verifier = new PactVerifier("PaymentService", config);

        verifier
            .WithMessages(scenarios =>
            {
                scenarios.Add("a PaymentSucceeded event", () => new PaymentSucceeded
                {
                    OrderId = Guid.Parse("d290f1ee-6c54-4b01-90e6-d701748f0851")
                });

                scenarios.Add("a PaymentFailed event", () => new PaymentFailed
                {
                    OrderId = Guid.Parse("d290f1ee-6c54-4b01-90e6-d701748f0851"),
                    Reason = "Card declined"
                });
            }, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            })
            .WithFileSource(new FileInfo(pactPath))
            .Verify();
    }
}
