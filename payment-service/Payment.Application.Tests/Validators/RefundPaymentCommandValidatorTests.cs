using FluentAssertions;
using Payment.Application.Commands;
using Payment.Application.Validators;

namespace Payment.Application.Tests.Validators;

public class RefundPaymentCommandValidatorTests
{
    private readonly RefundPaymentCommandValidator _validator = new();

    [Fact]
    public void Valid_PaymentId_ShouldPass()
    {
        var command = new RefundPaymentCommand(1);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Zero_PaymentId_ShouldFail()
    {
        var command = new RefundPaymentCommand(0);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Negative_PaymentId_ShouldFail()
    {
        var command = new RefundPaymentCommand(-1);
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
    }
}
