using Ecommerce.Model.Product.Request;
using FluentAssertions;
using Product.Application.Commands;
using Product.Application.Validators;

namespace Product.Application.Tests.Validators;

public class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator = new();

    [Fact]
    public void Valid_Command_ShouldPass()
    {
        var command = new CreateProductCommand(new CreateProductRequest
        {
            Name = "Valid Product",
            Description = "A valid product",
            Price = 10.00m
        });

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_Name_ShouldFail()
    {
        var command = new CreateProductCommand(new CreateProductRequest
        {
            Name = "",
            Description = "Test",
            Price = 10.00m
        });

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Name"));
    }

    [Fact]
    public void Zero_Price_ShouldFail()
    {
        var command = new CreateProductCommand(new CreateProductRequest
        {
            Name = "Test",
            Description = "Test",
            Price = 0
        });

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Price"));
    }
}
