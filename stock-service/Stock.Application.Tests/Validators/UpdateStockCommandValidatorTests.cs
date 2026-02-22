using FluentAssertions;
using Stock.Application.Commands;
using Stock.Application.Validators;

namespace Stock.Application.Tests.Validators;

public class UpdateStockCommandValidatorTests
{
    private readonly UpdateStockCommandValidator _validator = new();

    [Fact]
    public void Valid_Command_ShouldPass()
    {
        var command = new UpdateStockCommand(1, 100);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Zero_Quantity_ShouldPass()
    {
        var command = new UpdateStockCommand(1, 0);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Negative_Quantity_ShouldFail()
    {
        var command = new UpdateStockCommand(1, -1);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Zero_ProductId_ShouldFail()
    {
        var command = new UpdateStockCommand(0, 100);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }
}
