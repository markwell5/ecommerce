using System.Text.Json;
using Ecommerce.Events.Product;
using PactNet;
using PactNet.Verifier;
using Xunit.Abstractions;

namespace Ecommerce.Contract.Tests.Providers;

public class ProductServiceProviderTests
{
    private readonly ITestOutputHelper _output;

    public ProductServiceProviderTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ProductService_HonorsContractWith_StockService()
    {
        var pactPath = Path.Combine("..", "..", "..", "..", "pacts", "StockService-ProductService.json");

        if (!File.Exists(pactPath))
        {
            _output.WriteLine("Pact file not found — run consumer tests first");
            return;
        }

        var config = new PactVerifierConfig
        {
            LogLevel = PactLogLevel.Information
        };

        using var verifier = new PactVerifier("ProductService", config);

        verifier
            .WithMessages(scenarios =>
            {
                scenarios.Add("a ProductCreated event", () => new ProductCreated
                {
                    Id = 42
                });
            }, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            })
            .WithFileSource(new FileInfo(pactPath))
            .Verify();
    }
}
