using AutoMapper;
using Ecommerce.Model.User.Response;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using User.Application.Entities;

namespace User.Application.Queries
{
    public class GetUsersQuery : IRequest<GetUsersResult>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string Search { get; set; } = string.Empty;
    }

    public class GetUsersResult
    {
        public List<UserResponse> Users { get; set; } = [];
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, GetUsersResult>
    {
        private readonly UserDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public GetUsersQueryHandler(UserDbContext dbContext, UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<GetUsersResult> Handle(GetUsersQuery query, CancellationToken cancellationToken)
        {
            var usersQuery = _dbContext.Users.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.ToLowerInvariant();
                usersQuery = usersQuery.Where(u =>
                    u.Email.ToLower().Contains(search) ||
                    u.FirstName.ToLower().Contains(search) ||
                    u.LastName.ToLower().Contains(search));
            }

            var totalCount = await usersQuery.CountAsync(cancellationToken);

            var users = await usersQuery
                .OrderByDescending(u => u.CreatedAt)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

            var responses = new List<UserResponse>();
            foreach (var user in users)
            {
                var response = _mapper.Map<UserResponse>(user);
                var roles = await _userManager.GetRolesAsync(user);
                response.Role = roles.FirstOrDefault() ?? "User";
                responses.Add(response);
            }

            return new GetUsersResult
            {
                Users = responses,
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize
            };
        }
    }
}
