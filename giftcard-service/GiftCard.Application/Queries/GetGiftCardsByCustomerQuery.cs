using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Model.GiftCard.Response;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiftCard.Application.Queries
{
    public record GetGiftCardsByCustomerQuery(string CustomerId) : IRequest<List<GiftCardResponse>>;

    public class GetGiftCardsByCustomerQueryHandler : IRequestHandler<GetGiftCardsByCustomerQuery, List<GiftCardResponse>>
    {
        private readonly GiftCardDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetGiftCardsByCustomerQueryHandler(GiftCardDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<List<GiftCardResponse>> Handle(GetGiftCardsByCustomerQuery request, CancellationToken cancellationToken)
        {
            var giftCards = await _dbContext.GiftCards
                .AsNoTracking()
                .Where(g => g.PurchasedByCustomerId == request.CustomerId)
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<GiftCardResponse>>(giftCards);
        }
    }
}
