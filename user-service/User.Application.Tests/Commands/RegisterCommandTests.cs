using Ecommerce.Events.User;
using Ecommerce.Model.User.Request;
using Ecommerce.Model.User.Response;
using FluentAssertions;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using User.Application.Commands;
using User.Application.Entities;
using User.Application.Services;

namespace User.Application.Tests.Commands
{
    public class RegisterCommandTests
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IPublishEndpoint _publishEndpoint;

        public RegisterCommandTests()
        {
            var store = Substitute.For<IUserStore<ApplicationUser>>();
            _userManager = Substitute.For<UserManager<ApplicationUser>>(
                store, null, null, null, null, null, null, null, null);

            _tokenService = Substitute.For<ITokenService>();
            _publishEndpoint = Substitute.For<IPublishEndpoint>();
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldCreateUserAndReturnTokens()
        {
            var request = new RegisterRequest
            {
                Email = "test@example.com",
                Password = "Password123",
                FirstName = "John",
                LastName = "Doe",
                Phone = "07123456789"
            };

            _userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
                .Returns(IdentityResult.Success);
            _userManager.AddToRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
                .Returns(IdentityResult.Success);

            var expectedResponse = new AuthResponse
            {
                Token = "jwt-token",
                RefreshToken = "refresh-token",
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            };
            _tokenService.GenerateTokensAsync(Arg.Any<ApplicationUser>(), Arg.Any<IList<string>>())
                .Returns(expectedResponse);

            var handler = new RegisterCommandHandler(_userManager, _tokenService, _publishEndpoint);
            var result = await handler.Handle(new RegisterCommand(request), CancellationToken.None);

            result.Should().NotBeNull();
            result.Token.Should().Be("jwt-token");

            await _userManager.Received(1).CreateAsync(
                Arg.Is<ApplicationUser>(u => u.Email == "test@example.com" && u.FirstName == "John"),
                Arg.Is("Password123"));
            await _userManager.Received(1).AddToRoleAsync(Arg.Any<ApplicationUser>(), "User");
        }

        [Fact]
        public async Task Handle_ValidRequest_ShouldPublishUserRegisteredEvent()
        {
            var request = new RegisterRequest
            {
                Email = "test@example.com",
                Password = "Password123",
                FirstName = "John",
                LastName = "Doe"
            };

            _userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
                .Returns(IdentityResult.Success);
            _userManager.AddToRoleAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
                .Returns(IdentityResult.Success);

            _tokenService.GenerateTokensAsync(Arg.Any<ApplicationUser>(), Arg.Any<IList<string>>())
                .Returns(new AuthResponse());

            var handler = new RegisterCommandHandler(_userManager, _tokenService, _publishEndpoint);
            await handler.Handle(new RegisterCommand(request), CancellationToken.None);

            await _publishEndpoint.Received(1).Publish(
                Arg.Is<UserRegistered>(e => e.Email == "test@example.com"),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_DuplicateEmail_ShouldThrowValidationException()
        {
            var request = new RegisterRequest
            {
                Email = "existing@example.com",
                Password = "Password123",
                FirstName = "John",
                LastName = "Doe"
            };

            _userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
                .Returns(IdentityResult.Failed(new IdentityError
                {
                    Code = "DuplicateEmail",
                    Description = "Email already exists"
                }));

            var handler = new RegisterCommandHandler(_userManager, _tokenService, _publishEndpoint);

            var act = () => handler.Handle(new RegisterCommand(request), CancellationToken.None);
            await act.Should().ThrowAsync<Ecommerce.Shared.Infrastructure.Validation.ValidationException>();
        }
    }
}
