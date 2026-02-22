using AutoMapper;
using Ecommerce.Model.User.Response;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace User.Application.Queries
{
    public record GetAddressesQuery(Guid UserId) : IRequest<List<AddressResponse>>;

    public class GetAddressesQueryHandler : IRequestHandler<GetAddressesQuery, List<AddressResponse>>
    {
        private readonly UserDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetAddressesQueryHandler(UserDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<List<AddressResponse>> Handle(GetAddressesQuery query, CancellationToken cancellationToken)
        {
            var addresses = await _dbContext.Addresses
                .Where(a => a.UserId == query.UserId)
                .OrderByDescending(a => a.IsDefault)
                .ThenBy(a => a.Id)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<AddressResponse>>(addresses);
        }
    }
}
