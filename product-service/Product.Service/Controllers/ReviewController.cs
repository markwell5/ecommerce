using System.Security.Claims;
using System.Threading.Tasks;
using Asp.Versioning;
using Ecommerce.Model;
using Ecommerce.Model.Review.Request;
using Ecommerce.Model.Review.Response;
using Ecommerce.Shared.Infrastructure.RateLimiting;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Product.Application.Commands;
using Product.Application.Queries;

namespace Product.Service.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/reviews")]
    [EnableRateLimiting(RateLimitPolicies.Read)]
    public class ReviewController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReviewController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("product/{productId}")]
        [ProducesResponseType(200, Type = typeof(PagedResponse<ReviewResponse>))]
        public async Task<IActionResult> GetProductReviews(
            long productId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var reviews = await _mediator.Send(new GetProductReviewsQuery(productId, page, pageSize));
            return Ok(reviews);
        }

        [HttpGet("product/{productId}/rating")]
        [ProducesResponseType(200, Type = typeof(ProductRatingResponse))]
        public async Task<IActionResult> GetProductRating(long productId)
        {
            var rating = await _mediator.Send(new GetProductRatingQuery(productId));
            return Ok(rating);
        }

        [HttpPost]
        [Authorize]
        [EnableRateLimiting(RateLimitPolicies.Write)]
        [ProducesResponseType(201, Type = typeof(ReviewResponse))]
        [ProducesResponseType(409)]
        public async Task<IActionResult> Create([FromBody] CreateReviewRequest req)
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var review = await _mediator.Send(new CreateReviewCommand(customerId, req));

            if (review == null)
                return Conflict("You have already reviewed this product");

            return Created(review.Id.ToString(), review);
        }
    }
}
