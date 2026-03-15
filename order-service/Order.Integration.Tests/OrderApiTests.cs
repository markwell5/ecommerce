using System.Net;
using System.Net.Http.Json;
using Ecommerce.Model.Order.Request;
using Ecommerce.Model.Order.Response;
using FluentAssertions;

namespace Order.Integration.Tests;

public class OrderApiTests : IClassFixture<OrderServiceFactory>
{
    private readonly HttpClient _client;

    public OrderApiTests(OrderServiceFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PlaceOrder_ReturnsAccepted()
    {
        var request = new PlaceOrderRequest
        {
            CustomerId = "customer-1",
            Items = new List<OrderLineItem>
            {
                new()
                {
                    ProductId = 1,
                    ProductName = "Test Product",
                    Quantity = 2,
                    UnitPrice = 10.00m
                }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/v1/orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();
        order.Should().NotBeNull();
        order!.OrderId.Should().NotBeEmpty();
        order.CustomerId.Should().Be("customer-1");
        order.TotalAmount.Should().Be(20.00m);
    }

    [Fact]
    public async Task PlaceOrder_ReturnsCorrectTotalAndItems()
    {
        var request = new PlaceOrderRequest
        {
            CustomerId = "customer-2",
            Items = new List<OrderLineItem>
            {
                new()
                {
                    ProductId = 2,
                    ProductName = "Another Product",
                    Quantity = 3,
                    UnitPrice = 25.00m
                },
                new()
                {
                    ProductId = 3,
                    ProductName = "Third Product",
                    Quantity = 1,
                    UnitPrice = 10.00m
                }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/v1/orders", request);

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();
        order.Should().NotBeNull();
        order!.CustomerId.Should().Be("customer-2");
        order.TotalAmount.Should().Be(85.00m);
        order.Status.Should().Be("Placed");
    }

    [Fact]
    public async Task GetOrder_NonExistent_Returns404()
    {
        var response = await _client.GetAsync($"/api/v1/orders/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
