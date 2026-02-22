using Ecommerce.Model.User.Response;
using MediatR;
using User.Application.Services;

namespace User.Application.Commands
{
    public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponse>;

    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
    {
        private readonly ITokenService _tokenService;

        public RefreshTokenCommandHandler(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        public async Task<AuthResponse> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
        {
            return await _tokenService.RefreshTokenAsync(command.RefreshToken);
        }
    }
}
