using Ecommerce.Model.User.Request;
using Ecommerce.Model.User.Response;
using MediatR;
using Microsoft.AspNetCore.Identity;
using User.Application.Entities;
using User.Application.Services;

namespace User.Application.Commands
{
    public record LoginCommand(LoginRequest Request) : IRequest<AuthResponse>;

    public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;

        public LoginCommandHandler(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        public async Task<AuthResponse> Handle(LoginCommand command, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(command.Request.Email);
            if (user == null)
                return null;

            var isValid = await _userManager.CheckPasswordAsync(user, command.Request.Password);
            if (!isValid)
                return null;

            return await _tokenService.GenerateTokensAsync(user);
        }
    }
}
