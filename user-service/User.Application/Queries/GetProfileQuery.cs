using AutoMapper;
using Ecommerce.Model.User.Response;
using MediatR;
using Microsoft.AspNetCore.Identity;
using User.Application.Entities;

namespace User.Application.Queries
{
    public record GetProfileQuery(Guid UserId) : IRequest<UserResponse>;

    public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, UserResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public GetProfileQueryHandler(UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<UserResponse> Handle(GetProfileQuery query, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(query.UserId.ToString());
            if (user == null)
                return null;

            var response = _mapper.Map<UserResponse>(user);
            var roles = await _userManager.GetRolesAsync(user);
            response.Role = roles.FirstOrDefault() ?? "User";
            return response;
        }
    }
}
