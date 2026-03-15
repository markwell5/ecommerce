using System.Text.Json;
using System.Text.Json.Nodes;
using Ecommerce.Events.Product;
using FluentAssertions;
using Xunit.Abstractions;

namespace Ecommerce.Contract.Tests.Providers;

public class ProductServiceProviderTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly ITestOutputHelper _output;

    public ProductServiceProviderTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ProductService_ProducesValidMessage_ProductCreated()
    {
        var message = new ProductCreated
        {
            Id = 42
        };

        var json = JsonSerializer.Serialize(message, JsonOptions);
        var node = JsonNode.Parse(json)!;

        node["id"].Should().NotBeNull();

        VerifyAgainstPact("StockService-ProductService.json", "a ProductCreated event", node);
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
