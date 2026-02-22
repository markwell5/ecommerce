using Ecommerce.Model.User.Request;
using Ecommerce.Model.User.Response;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using User.Application.Commands;
using User.Application.Entities;
using User.Application.Services;

namespace User.Application.Tests.Commands
{
    public class LoginCommandTests
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;

        public LoginCommandTests()
        {
            var store = Substitute.For<IUserStore<ApplicationUser>>();
            _userManager = Substitute.For<UserManager<ApplicationUser>>(
                store, null, null, null, null, null, null, null, null);

            _tokenService = Substitute.For<ITokenService>();
        }

        [Fact]
        public async Task Handle_ValidCredentials_ShouldReturnTokens()
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe"
            };

            _userManager.FindByEmailAsync("test@example.com").Returns(user);
            _userManager.CheckPasswordAsync(user, "Password123").Returns(true);

            var expectedResponse = new AuthResponse
            {
                Token = "jwt-token",
                RefreshToken = "refresh-token",
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            };
            _tokenService.GenerateTokensAsync(user).Returns(expectedResponse);

            var handler = new LoginCommandHandler(_userManager, _tokenService);
            var result = await handler.Handle(
                new LoginCommand(new LoginRequest { Email = "test@example.com", Password = "Password123" }),
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Token.Should().Be("jwt-token");
        }

        [Fact]
        public async Task Handle_InvalidEmail_ShouldReturnNull()
        {
            _userManager.FindByEmailAsync("wrong@example.com").Returns((ApplicationUser)null);

            var handler = new LoginCommandHandler(_userManager, _tokenService);
            var result = await handler.Handle(
                new LoginCommand(new LoginRequest { Email = "wrong@example.com", Password = "Password123" }),
                CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_InvalidPassword_ShouldReturnNull()
        {
            var user = new ApplicationUser { Email = "test@example.com" };
            _userManager.FindByEmailAsync("test@example.com").Returns(user);
            _userManager.CheckPasswordAsync(user, "wrongpassword").Returns(false);

            var handler = new LoginCommandHandler(_userManager, _tokenService);
            var result = await handler.Handle(
                new LoginCommand(new LoginRequest { Email = "test@example.com", Password = "wrongpassword" }),
                CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
