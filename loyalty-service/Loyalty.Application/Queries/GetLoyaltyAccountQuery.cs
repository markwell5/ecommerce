using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Model.Loyalty.Response;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Loyalty.Application.Queries
{
    public record GetLoyaltyAccountQuery(string CustomerId) : IRequest<LoyaltyAccountResponse?>;

    public class GetLoyaltyAccountQueryHandler : IRequestHandler<GetLoyaltyAccountQuery, LoyaltyAccountResponse?>
    {
        private readonly LoyaltyDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetLoyaltyAccountQueryHandler(LoyaltyDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<LoyaltyAccountResponse?> Handle(GetLoyaltyAccountQuery request, CancellationToken cancellationToken)
        {
            var account = await _dbContext.LoyaltyAccounts
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.CustomerId == request.CustomerId, cancellationToken);

            return account == null ? null : _mapper.Map<LoyaltyAccountResponse>(account);
        }
    }
}
