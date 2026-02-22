using Ecommerce.Model.User.Request;
using FluentAssertions;
using User.Application.Commands;
using User.Application.Validators;

namespace User.Application.Tests.Validators
{
    public class AddressCommandValidatorTests
    {
        private readonly AddAddressCommandValidator _validator = new();

        [Fact]
        public void Valid_Command_ShouldPass()
        {
            var command = new AddAddressCommand(Guid.NewGuid(), new AddressRequest
            {
                Line1 = "123 Main St",
                City = "London",
                PostCode = "SW1A 1AA",
                Country = "United Kingdom"
            });

            var result = _validator.Validate(command);
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Empty_Line1_ShouldFail()
        {
            var command = new AddAddressCommand(Guid.NewGuid(), new AddressRequest
            {
                Line1 = "",
                City = "London",
                PostCode = "SW1A 1AA",
                Country = "United Kingdom"
            });

            var result = _validator.Validate(command);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName.Contains("Line1"));
        }

        [Fact]
        public void Empty_City_ShouldFail()
        {
            var command = new AddAddressCommand(Guid.NewGuid(), new AddressRequest
            {
                Line1 = "123 Main St",
                City = "",
                PostCode = "SW1A 1AA",
                Country = "United Kingdom"
            });

            var result = _validator.Validate(command);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName.Contains("City"));
        }

        [Fact]
        public void Empty_PostCode_ShouldFail()
        {
            var command = new AddAddressCommand(Guid.NewGuid(), new AddressRequest
            {
                Line1 = "123 Main St",
                City = "London",
                PostCode = "",
                Country = "United Kingdom"
            });

            var result = _validator.Validate(command);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName.Contains("PostCode"));
        }

        [Fact]
        public void Empty_Country_ShouldFail()
        {
            var command = new AddAddressCommand(Guid.NewGuid(), new AddressRequest
            {
                Line1 = "123 Main St",
                City = "London",
                PostCode = "SW1A 1AA",
                Country = ""
            });

            var result = _validator.Validate(command);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName.Contains("Country"));
        }
    }
}
