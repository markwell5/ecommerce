using AutoMapper;
using Ecommerce.Events.User;
using Ecommerce.Model.User.Request;
using Ecommerce.Model.User.Response;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Identity;
using User.Application.Entities;

namespace User.Application.Commands
{
    public record UpdateProfileCommand(Guid UserId, UpdateProfileRequest Request) : IRequest<UserResponse>;

    public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, UserResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publishEndpoint;

        public UpdateProfileCommandHandler(
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            IPublishEndpoint publishEndpoint)
        {
            _userManager = userManager;
            _mapper = mapper;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<UserResponse> Handle(UpdateProfileCommand command, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(command.UserId.ToString());
            if (user == null)
                return null;

            user.FirstName = command.Request.FirstName;
            user.LastName = command.Request.LastName;
            user.PhoneNumber = command.Request.Phone;
            user.UpdatedAt = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);

            await _publishEndpoint.Publish(new UserUpdated
            {
                UserId = user.Id
            }, cancellationToken);

            return _mapper.Map<UserResponse>(user);
        }
    }
}
