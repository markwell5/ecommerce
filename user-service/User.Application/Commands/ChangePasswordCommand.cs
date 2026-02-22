using Ecommerce.Model.User.Request;
using MediatR;
using Microsoft.AspNetCore.Identity;
using User.Application.Entities;

namespace User.Application.Commands
{
    public record ChangePasswordCommand(Guid UserId, ChangePasswordRequest Request) : IRequest<bool>;

    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ChangePasswordCommandHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(command.UserId.ToString());
            if (user == null)
                return false;

            var result = await _userManager.ChangePasswordAsync(
                user, command.Request.CurrentPassword, command.Request.NewPassword);

            return result.Succeeded;
        }
    }
}
