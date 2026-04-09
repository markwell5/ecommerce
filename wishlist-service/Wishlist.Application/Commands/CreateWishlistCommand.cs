using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Model.Wishlist.Response;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Wishlist.Application.Commands
{
    public class CreateWishlistCommand : IRequest<WishlistResponse>
    {
        public string CustomerId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class CreateWishlistCommandHandler : IRequestHandler<CreateWishlistCommand, WishlistResponse>
    {
        private readonly WishlistDbContext _dbContext;
        private readonly IMapper _mapper;

        public CreateWishlistCommandHandler(WishlistDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<WishlistResponse> Handle(CreateWishlistCommand command, CancellationToken cancellationToken)
        {
            var wishlist = new Entities.Wishlist
            {
                CustomerId = command.CustomerId,
                Name = command.Name,
                IsDefault = false
            };

            _dbContext.Wishlists.Add(wishlist);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return _mapper.Map<WishlistResponse>(wishlist);
        }
    }
}
