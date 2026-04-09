using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Model.GiftCard.Response;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiftCard.Application.Queries
{
    public record GetGiftCardByCodeQuery(string Code) : IRequest<GiftCardResponse?>;

    public class GetGiftCardByCodeQueryHandler : IRequestHandler<GetGiftCardByCodeQuery, GiftCardResponse?>
    {
        private readonly GiftCardDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetGiftCardByCodeQueryHandler(GiftCardDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<GiftCardResponse?> Handle(GetGiftCardByCodeQuery request, CancellationToken cancellationToken)
        {
            var giftCard = await _dbContext.GiftCards
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Code == request.Code, cancellationToken);

            return giftCard == null ? null : _mapper.Map<GiftCardResponse>(giftCard);
        }
    }
}
