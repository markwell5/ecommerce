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
    public record GetGiftCardTransactionsQuery(string Code, int Page, int PageSize) : IRequest<GiftCardTransactionHistoryResponse>;

    public class GetGiftCardTransactionsQueryHandler : IRequestHandler<GetGiftCardTransactionsQuery, GiftCardTransactionHistoryResponse>
    {
        private readonly GiftCardDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetGiftCardTransactionsQueryHandler(GiftCardDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<GiftCardTransactionHistoryResponse> Handle(GetGiftCardTransactionsQuery request, CancellationToken cancellationToken)
        {
            var giftCard = await _dbContext.GiftCards
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Code == request.Code, cancellationToken);

            if (giftCard == null)
            {
                return new GiftCardTransactionHistoryResponse
                {
                    Items = new List<GiftCardTransactionResponse>(),
                    TotalCount = 0,
                    Page = request.Page,
                    PageSize = request.PageSize
                };
            }

            var query = _dbContext.GiftCardTransactions
                .AsNoTracking()
                .Where(t => t.GiftCardId == giftCard.Id);

            var totalCount = await query.CountAsync(cancellationToken);

            var transactions = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new GiftCardTransactionHistoryResponse
            {
                Items = _mapper.Map<List<GiftCardTransactionResponse>>(transactions),
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }
    }
}
