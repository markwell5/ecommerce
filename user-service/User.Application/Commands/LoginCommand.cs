using Ecommerce.Model.User.Request;
using Ecommerce.Model.User.Response;
using Ecommerce.Shared.Infrastructure.Audit;
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
        private readonly IAuditPublisher _auditPublisher;

        public LoginCommandHandler(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService,
            IAuditPublisher auditPublisher)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _auditPublisher = auditPublisher;
        }

        public async Task<AuthResponse> Handle(LoginCommand command, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(command.Request.Email);
            if (user == null)
                return null;

            var isValid = await _userManager.CheckPasswordAsync(user, command.Request.Password);
            if (!isValid)
            {
                await _auditPublisher.PublishAsync("LoginFailed", "User", command.Request.Email, "");
                return null;
            }

            await _auditPublisher.PublishAsync("Login", "User", user.Id.ToString(), user.Id.ToString());
            return await _tokenService.GenerateTokensAsync(user);
        }
    }
}
