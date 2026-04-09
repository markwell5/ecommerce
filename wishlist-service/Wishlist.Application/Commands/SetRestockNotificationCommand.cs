using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Model.Wishlist.Response;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Wishlist.Application.Commands
{
    public class SetRestockNotificationCommand : IRequest<WishlistResponse>
    {
        public long WishlistId { get; set; }
        public long ProductId { get; set; }
        public bool NotifyOnRestock { get; set; }
    }

    public class SetRestockNotificationCommandHandler : IRequestHandler<SetRestockNotificationCommand, WishlistResponse>
    {
        private readonly WishlistDbContext _dbContext;
        private readonly IMapper _mapper;

        public SetRestockNotificationCommandHandler(WishlistDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<WishlistResponse> Handle(SetRestockNotificationCommand command, CancellationToken cancellationToken)
        {
            var item = await _dbContext.WishlistItems
                .FirstOrDefaultAsync(i => i.WishlistId == command.WishlistId && i.ProductId == command.ProductId, cancellationToken)
                ?? throw new InvalidOperationException($"Item with ProductId {command.ProductId} not found in wishlist {command.WishlistId}");

            item.NotifyOnRestock = command.NotifyOnRestock;

            var wishlist = await _dbContext.Wishlists
                .Include(w => w.Items)
                .FirstAsync(w => w.Id == command.WishlistId, cancellationToken);

            wishlist.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return _mapper.Map<WishlistResponse>(wishlist);
        }
    }
}
