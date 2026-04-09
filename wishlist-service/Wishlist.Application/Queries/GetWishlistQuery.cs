using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Model.Wishlist.Response;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Wishlist.Application.Queries
{
    public record GetWishlistQuery(long Id) : IRequest<WishlistResponse?>;

    public class GetWishlistQueryHandler : IRequestHandler<GetWishlistQuery, WishlistResponse?>
    {
        private readonly WishlistDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetWishlistQueryHandler(WishlistDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<WishlistResponse?> Handle(GetWishlistQuery request, CancellationToken cancellationToken)
        {
            var wishlist = await _dbContext.Wishlists
                .AsNoTracking()
                .Include(w => w.Items)
                .FirstOrDefaultAsync(w => w.Id == request.Id, cancellationToken);

            return wishlist == null ? null : _mapper.Map<WishlistResponse>(wishlist);
        }
    }
}
