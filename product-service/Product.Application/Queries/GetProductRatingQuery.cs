using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ecommerce.Model.Review.Response;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Product.Application.Queries
{
    public class GetProductRatingQuery : IRequest<ProductRatingResponse>
    {
        public GetProductRatingQuery(long productId)
        {
            ProductId = productId;
        }

        public long ProductId { get; }
    }

    public class GetProductRatingQueryHandler : IRequestHandler<GetProductRatingQuery, ProductRatingResponse>
    {
        private readonly ProductDbContext _dbContext;

        public GetProductRatingQueryHandler(ProductDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ProductRatingResponse> Handle(GetProductRatingQuery request, CancellationToken cancellationToken)
        {
            var reviews = await _dbContext.Reviews
                .AsNoTracking()
                .Where(r => r.ProductId == request.ProductId)
                .Select(r => r.Rating)
                .ToListAsync(cancellationToken);

            return new ProductRatingResponse
            {
                ProductId = request.ProductId,
                AverageRating = reviews.Count > 0 ? reviews.Average() : 0,
                ReviewCount = reviews.Count
            };
        }
    }
}
