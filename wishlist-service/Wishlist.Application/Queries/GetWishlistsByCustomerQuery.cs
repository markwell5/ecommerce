using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Model.Wishlist.Response;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Wishlist.Application.Queries
{
    public record GetWishlistsByCustomerQuery(string CustomerId) : IRequest<List<WishlistResponse>>;

    public class GetWishlistsByCustomerQueryHandler : IRequestHandler<GetWishlistsByCustomerQuery, List<WishlistResponse>>
    {
        private readonly WishlistDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetWishlistsByCustomerQueryHandler(WishlistDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<List<WishlistResponse>> Handle(GetWishlistsByCustomerQuery request, CancellationToken cancellationToken)
        {
            var wishlists = await _dbContext.Wishlists
                .AsNoTracking()
                .Include(w => w.Items)
                .Where(w => w.CustomerId == request.CustomerId)
                .OrderByDescending(w => w.IsDefault)
                .ThenByDescending(w => w.CreatedAt)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<WishlistResponse>>(wishlists);
        }
    }
}
