using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Model.Wishlist.Response;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Wishlist.Application.Commands
{
    public class RemoveWishlistItemCommand : IRequest<WishlistResponse>
    {
        public long WishlistId { get; set; }
        public long ProductId { get; set; }
    }

    public class RemoveWishlistItemCommandHandler : IRequestHandler<RemoveWishlistItemCommand, WishlistResponse>
    {
        private readonly WishlistDbContext _dbContext;
        private readonly IMapper _mapper;

        public RemoveWishlistItemCommandHandler(WishlistDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<WishlistResponse> Handle(RemoveWishlistItemCommand command, CancellationToken cancellationToken)
        {
            var item = await _dbContext.WishlistItems
                .FirstOrDefaultAsync(i => i.WishlistId == command.WishlistId && i.ProductId == command.ProductId, cancellationToken)
                ?? throw new InvalidOperationException($"Item with ProductId {command.ProductId} not found in wishlist {command.WishlistId}");

            _dbContext.WishlistItems.Remove(item);

            var wishlist = await _dbContext.Wishlists
                .Include(w => w.Items)
                .FirstAsync(w => w.Id == command.WishlistId, cancellationToken);

            wishlist.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return _mapper.Map<WishlistResponse>(wishlist);
        }
    }
}
