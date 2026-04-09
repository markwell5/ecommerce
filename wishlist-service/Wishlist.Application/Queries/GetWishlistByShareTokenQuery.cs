using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Model.Wishlist.Response;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Wishlist.Application.Queries
{
    public record GetWishlistByShareTokenQuery(string ShareToken) : IRequest<WishlistResponse?>;

    public class GetWishlistByShareTokenQueryHandler : IRequestHandler<GetWishlistByShareTokenQuery, WishlistResponse?>
    {
        private readonly WishlistDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetWishlistByShareTokenQueryHandler(WishlistDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<WishlistResponse?> Handle(GetWishlistByShareTokenQuery request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(request.ShareToken, out var token))
                return null;

            var wishlist = await _dbContext.Wishlists
                .AsNoTracking()
                .Include(w => w.Items)
                .FirstOrDefaultAsync(w => w.ShareToken == token && w.IsPublic, cancellationToken);

            return wishlist == null ? null : _mapper.Map<WishlistResponse>(wishlist);
        }
    }
}
