using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Wishlist.Application.Commands
{
    public class DeleteWishlistCommand : IRequest<bool>
    {
        public long Id { get; set; }
    }

    public class DeleteWishlistCommandHandler : IRequestHandler<DeleteWishlistCommand, bool>
    {
        private readonly WishlistDbContext _dbContext;

        public DeleteWishlistCommandHandler(WishlistDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> Handle(DeleteWishlistCommand command, CancellationToken cancellationToken)
        {
            var wishlist = await _dbContext.Wishlists
                .FirstOrDefaultAsync(w => w.Id == command.Id, cancellationToken)
                ?? throw new InvalidOperationException($"Wishlist {command.Id} not found");

            if (wishlist.IsDefault)
                throw new InvalidOperationException("Cannot delete the default wishlist");

            _dbContext.Wishlists.Remove(wishlist);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
