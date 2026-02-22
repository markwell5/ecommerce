using Ecommerce.Model.Order.Request;
using FluentAssertions;
using Order.Application.Commands;
using Order.Application.Validators;

namespace Order.Application.Tests.Validators;

public class PlaceOrderCommandValidatorTests
{
    private readonly PlaceOrderCommandValidator _validator = new();

    [Fact]
    public void Valid_Command_ShouldPass()
    {
        var command = new PlaceOrderCommand(new PlaceOrderRequest
        {
            CustomerId = "customer-1",
            Items = new List<OrderLineItem>
            {
                new() { ProductId = 1, ProductName = "Apple", Quantity = 1, UnitPrice = 1.50m }
            }
        });

        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_CustomerId_ShouldFail()
    {
        var command = new PlaceOrderCommand(new PlaceOrderRequest
        {
            CustomerId = "",
            Items = new List<OrderLineItem>
            {
                new() { ProductId = 1, ProductName = "Apple", Quantity = 1, UnitPrice = 1.50m }
            }
        });

        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("CustomerId"));
    }

    [Fact]
    public void Empty_Items_ShouldFail()
    {
        var command = new PlaceOrderCommand(new PlaceOrderRequest
        {
            CustomerId = "customer-1",
            Items = new List<OrderLineItem>()
        });

        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Items"));
    }

    [Fact]
    public void Zero_Quantity_ShouldFail()
    {
        var command = new PlaceOrderCommand(new PlaceOrderRequest
        {
            CustomerId = "customer-1",
            Items = new List<OrderLineItem>
            {
                new() { ProductId = 1, ProductName = "Apple", Quantity = 0, UnitPrice = 1.50m }
            }
        });

        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }
}
