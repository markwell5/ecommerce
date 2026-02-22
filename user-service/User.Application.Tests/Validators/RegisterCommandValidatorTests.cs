using Ecommerce.Model.User.Request;
using FluentAssertions;
using User.Application.Commands;
using User.Application.Validators;

namespace User.Application.Tests.Validators
{
    public class RegisterCommandValidatorTests
    {
        private readonly RegisterCommandValidator _validator = new();

        [Fact]
        public void Valid_Command_ShouldPass()
        {
            var command = new RegisterCommand(new RegisterRequest
            {
                Email = "test@example.com",
                Password = "Password123",
                FirstName = "John",
                LastName = "Doe"
            });

            var result = _validator.Validate(command);
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Empty_Email_ShouldFail()
        {
            var command = new RegisterCommand(new RegisterRequest
            {
                Email = "",
                Password = "Password123",
                FirstName = "John",
                LastName = "Doe"
            });

            var result = _validator.Validate(command);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName.Contains("Email"));
        }

        [Fact]
        public void Invalid_Email_Format_ShouldFail()
        {
            var command = new RegisterCommand(new RegisterRequest
            {
                Email = "not-an-email",
                Password = "Password123",
                FirstName = "John",
                LastName = "Doe"
            });

            var result = _validator.Validate(command);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName.Contains("Email"));
        }

        [Fact]
        public void Short_Password_ShouldFail()
        {
            var command = new RegisterCommand(new RegisterRequest
            {
                Email = "test@example.com",
                Password = "short",
                FirstName = "John",
                LastName = "Doe"
            });

            var result = _validator.Validate(command);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName.Contains("Password"));
        }

        [Fact]
        public void Empty_FirstName_ShouldFail()
        {
            var command = new RegisterCommand(new RegisterRequest
            {
                Email = "test@example.com",
                Password = "Password123",
                FirstName = "",
                LastName = "Doe"
            });

            var result = _validator.Validate(command);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName.Contains("FirstName"));
        }

        [Fact]
        public void Empty_LastName_ShouldFail()
        {
            var command = new RegisterCommand(new RegisterRequest
            {
                Email = "test@example.com",
                Password = "Password123",
                FirstName = "John",
                LastName = ""
            });

            var result = _validator.Validate(command);
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName.Contains("LastName"));
        }
    }
}
