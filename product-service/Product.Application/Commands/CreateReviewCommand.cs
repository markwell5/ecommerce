using System;
using System.Threading;
using System.Threading.Tasks;
using Ecommerce.Events.Review;
using Ecommerce.Model.Review.Request;
using Ecommerce.Model.Review.Response;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Product.Application.Commands
{
    public class CreateReviewCommand : IRequest<ReviewResponse>
    {
        public CreateReviewCommand(string customerId, CreateReviewRequest request)
        {
            CustomerId = customerId;
            Request = request;
        }

        public string CustomerId { get; }
        public CreateReviewRequest Request { get; }
    }

    public class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, ReviewResponse>
    {
        private readonly ProductDbContext _dbContext;
        private readonly IPublishEndpoint _publishEndpoint;

        public CreateReviewCommandHandler(ProductDbContext dbContext, IPublishEndpoint publishEndpoint)
        {
            _dbContext = dbContext;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<ReviewResponse> Handle(CreateReviewCommand command, CancellationToken cancellationToken)
        {
            var exists = await _dbContext.Reviews
                .AnyAsync(r => r.ProductId == command.Request.ProductId && r.CustomerId == command.CustomerId, cancellationToken);

            if (exists)
                return null;

            var review = new Entities.Review
            {
                ProductId = command.Request.ProductId,
                CustomerId = command.CustomerId,
                Rating = command.Request.Rating,
                Title = command.Request.Title,
                Body = command.Request.Body,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Reviews.Add(review);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(new ReviewCreated
            {
                Id = review.Id,
                ProductId = review.ProductId,
                CustomerId = review.CustomerId,
                Rating = review.Rating
            }, cancellationToken);

            return new ReviewResponse
            {
                Id = review.Id,
                ProductId = review.ProductId,
                CustomerId = review.CustomerId,
                Rating = review.Rating,
                Title = review.Title,
                Body = review.Body,
                CreatedAt = review.CreatedAt
            };
        }
    }
}
