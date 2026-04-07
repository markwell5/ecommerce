using System.Text.Json;
using Ecommerce.Events.Order.Messages;
using Ecommerce.Model.Order.Request;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Order.Application.Commands;

namespace Order.Application.Tests.Commands;

public class PlaceOrderCommandTests
{
    private static OrderDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new OrderDbContext(options);
    }

    [Fact]
    public async Task Handle_ShouldReturnOrderResponse_WithCorrectTotalAmount()
    {
        var sendEndpoint = Substitute.For<ISendEndpoint>();
        var sendEndpointProvider = Substitute.For<ISendEndpointProvider>();
        sendEndpointProvider.GetSendEndpoint(Arg.Any<Uri>()).Returns(sendEndpoint);

        var handler = new PlaceOrderCommandHandler(sendEndpointProvider, CreateInMemoryDb(), NSubstitute.Substitute.For<Ecommerce.Shared.Infrastructure.Audit.IAuditPublisher>());
        var command = new PlaceOrderCommand(new PlaceOrderRequest
        {
            CustomerId = "customer-1",
            Items = new List<OrderLineItem>
            {
                new() { ProductId = 1, ProductName = "Apple", Quantity = 3, UnitPrice = 1.50m },
                new() { ProductId = 2, ProductName = "Banana", Quantity = 2, UnitPrice = 2.00m }
            }
        });

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.CustomerId.Should().Be("customer-1");
        result.TotalAmount.Should().Be(8.50m); // 3*1.50 + 2*2.00
        result.Status.Should().Be("Placed");
        result.OrderId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldSendPlaceOrderMessage()
    {
        var sendEndpoint = Substitute.For<ISendEndpoint>();
        var sendEndpointProvider = Substitute.For<ISendEndpointProvider>();
        sendEndpointProvider.GetSendEndpoint(Arg.Any<Uri>()).Returns(sendEndpoint);

        var handler = new PlaceOrderCommandHandler(sendEndpointProvider, CreateInMemoryDb(), NSubstitute.Substitute.For<Ecommerce.Shared.Infrastructure.Audit.IAuditPublisher>());
        var command = new PlaceOrderCommand(new PlaceOrderRequest
        {
            CustomerId = "customer-1",
            Items = new List<OrderLineItem>
            {
                new() { ProductId = 1, ProductName = "Apple", Quantity = 1, UnitPrice = 1.00m }
            }
        });

        await handler.Handle(command, CancellationToken.None);

        await sendEndpoint.Received(1).Send(
            Arg.Is<PlaceOrder>(m => m.CustomerId == "customer-1" && m.TotalAmount == 1.00m),
            Arg.Any<CancellationToken>());
    }
}
