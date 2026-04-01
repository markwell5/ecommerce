using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ecommerce.Model;
using Ecommerce.Model.Review.Response;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Product.Application.Queries
{
    public class GetProductReviewsQuery : IRequest<PagedResponse<ReviewResponse>>
    {
        public GetProductReviewsQuery(long productId, int page, int pageSize)
        {
            ProductId = productId;
            Page = page;
            PageSize = pageSize;
        }

        public long ProductId { get; }
        public int Page { get; }
        public int PageSize { get; }
    }

    public class GetProductReviewsQueryHandler : IRequestHandler<GetProductReviewsQuery, PagedResponse<ReviewResponse>>
    {
        private readonly ProductDbContext _dbContext;

        public GetProductReviewsQueryHandler(ProductDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PagedResponse<ReviewResponse>> Handle(GetProductReviewsQuery request, CancellationToken cancellationToken)
        {
            var page = Math.Max(1, request.Page);
            var pageSize = Math.Clamp(request.PageSize, 1, 50);

            var query = _dbContext.Reviews
                .AsNoTracking()
                .Where(r => r.ProductId == request.ProductId)
                .OrderByDescending(r => r.CreatedAt);

            var totalCount = await query.CountAsync(cancellationToken);
            var reviews = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ReviewResponse
                {
                    Id = r.Id,
                    ProductId = r.ProductId,
                    CustomerId = r.CustomerId,
                    Rating = r.Rating,
                    Title = r.Title,
                    Body = r.Body,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync(cancellationToken);

            return new PagedResponse<ReviewResponse>
            {
                Items = reviews.ToArray(),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }
    }
}
