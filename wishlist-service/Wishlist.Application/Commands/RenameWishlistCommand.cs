using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Model.Wishlist.Response;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Wishlist.Application.Commands
{
    public class RenameWishlistCommand : IRequest<WishlistResponse>
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class RenameWishlistCommandHandler : IRequestHandler<RenameWishlistCommand, WishlistResponse>
    {
        private readonly WishlistDbContext _dbContext;
        private readonly IMapper _mapper;

        public RenameWishlistCommandHandler(WishlistDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<WishlistResponse> Handle(RenameWishlistCommand command, CancellationToken cancellationToken)
        {
            var wishlist = await _dbContext.Wishlists
                .Include(w => w.Items)
                .FirstOrDefaultAsync(w => w.Id == command.Id, cancellationToken)
                ?? throw new InvalidOperationException($"Wishlist {command.Id} not found");

            wishlist.Name = command.Name;
            wishlist.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return _mapper.Map<WishlistResponse>(wishlist);
        }
    }
}
