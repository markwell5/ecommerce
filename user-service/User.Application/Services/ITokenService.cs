using Ecommerce.Model.User.Response;
using User.Application.Entities;

namespace User.Application.Services
{
    public interface ITokenService
    {
        Task<AuthResponse> GenerateTokensAsync(ApplicationUser user);
        Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    }
}
