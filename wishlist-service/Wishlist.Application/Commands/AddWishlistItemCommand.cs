using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Model.Wishlist.Response;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wishlist.Application.Entities;

namespace Wishlist.Application.Commands
{
    public class AddWishlistItemCommand : IRequest<WishlistResponse>
    {
        public long WishlistId { get; set; }
        public long ProductId { get; set; }
        public string CustomerId { get; set; } = string.Empty;
    }

    public class AddWishlistItemCommandHandler : IRequestHandler<AddWishlistItemCommand, WishlistResponse>
    {
        private readonly WishlistDbContext _dbContext;
        private readonly IMapper _mapper;

        public AddWishlistItemCommandHandler(WishlistDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<WishlistResponse> Handle(AddWishlistItemCommand command, CancellationToken cancellationToken)
        {
            Entities.Wishlist? wishlist;

            if (command.WishlistId > 0)
            {
                wishlist = await _dbContext.Wishlists
                    .Include(w => w.Items)
                    .FirstOrDefaultAsync(w => w.Id == command.WishlistId, cancellationToken)
                    ?? throw new InvalidOperationException($"Wishlist {command.WishlistId} not found");
            }
            else
            {
                // Auto-create default wishlist if none exists
                wishlist = await _dbContext.Wishlists
                    .Include(w => w.Items)
                    .FirstOrDefaultAsync(w => w.CustomerId == command.CustomerId && w.IsDefault, cancellationToken);

                if (wishlist == null)
                {
                    wishlist = new Entities.Wishlist
                    {
                        CustomerId = command.CustomerId,
                        Name = "Saved for Later",
                        IsDefault = true
                    };
                    _dbContext.Wishlists.Add(wishlist);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
            }

            var existingItem = await _dbContext.WishlistItems
                .FirstOrDefaultAsync(i => i.WishlistId == wishlist.Id && i.ProductId == command.ProductId, cancellationToken);

            if (existingItem == null)
            {
                var item = new WishlistItem
                {
                    WishlistId = wishlist.Id,
                    ProductId = command.ProductId
                };
                _dbContext.WishlistItems.Add(item);
                wishlist.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            // Reload with items
            wishlist = await _dbContext.Wishlists
                .Include(w => w.Items)
                .FirstAsync(w => w.Id == wishlist.Id, cancellationToken);

            return _mapper.Map<WishlistResponse>(wishlist);
        }
    }
}
