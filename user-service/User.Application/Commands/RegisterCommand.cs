using Ecommerce.Events.User;
using Ecommerce.Model.User.Request;
using Ecommerce.Model.User.Response;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Identity;
using User.Application.Entities;
using User.Application.Services;

namespace User.Application.Commands
{
    public record RegisterCommand(RegisterRequest Request) : IRequest<AuthResponse>;

    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IPublishEndpoint _publishEndpoint;

        public RegisterCommandHandler(
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService,
            IPublishEndpoint publishEndpoint)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<AuthResponse> Handle(RegisterCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.Phone
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.ToDictionary(
                    e => e.Code,
                    e => new[] { e.Description });
                throw new Ecommerce.Shared.Infrastructure.Validation.ValidationException(errors);
            }

            await _publishEndpoint.Publish(new UserRegistered
            {
                UserId = user.Id,
                Email = user.Email
            }, cancellationToken);

            return await _tokenService.GenerateTokensAsync(user);
        }
    }
}
