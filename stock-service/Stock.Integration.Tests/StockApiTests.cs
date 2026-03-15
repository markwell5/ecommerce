using System.Net;
using System.Net.Http.Json;
using Ecommerce.Model.Stock.Request;
using Ecommerce.Model.Stock.Response;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Stock.Application;
using Stock.Application.Entities;

namespace Stock.Integration.Tests;

public class StockApiTests : IClassFixture<StockServiceFactory>
{
    private readonly StockServiceFactory _factory;
    private readonly HttpClient _client;

    public StockApiTests(StockServiceFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<StockItem> SeedStockItem(long productId, int quantity)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StockDbContext>();
        var item = new StockItem
        {
            ProductId = productId,
            AvailableQuantity = quantity,
            ReservedQuantity = 0
        };
        db.StockItems.Add(item);
        await db.SaveChangesAsync();
        return item;
    }

    [Fact]
    public async Task GetStock_ExistingProduct_ReturnsStock()
    {
        await SeedStockItem(1001, 50);

        var response = await _client.GetAsync("/api/v1/stock/1001");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var stock = await response.Content.ReadFromJsonAsync<StockResponse>();
        stock!.ProductId.Should().Be(1001);
        stock.AvailableQuantity.Should().Be(50);
    }

    [Fact]
    public async Task GetStock_NonExistentProduct_Returns404()
    {
        var response = await _client.GetAsync("/api/v1/stock/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateStock_ExistingProduct_ReturnsUpdated()
    {
        await SeedStockItem(1002, 30);

        var request = new UpdateStockRequest { Quantity = 100 };
        var response = await _client.PutAsJsonAsync("/api/v1/stock/1002", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var stock = await response.Content.ReadFromJsonAsync<StockResponse>();
        stock!.AvailableQuantity.Should().Be(100);
    }

    [Fact]
    public async Task UpdateStock_NonExistentProduct_Returns404()
    {
        var request = new UpdateStockRequest { Quantity = 10 };
        var response = await _client.PutAsJsonAsync("/api/v1/stock/99998", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
