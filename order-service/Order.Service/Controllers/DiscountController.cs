using System.Collections.Generic;
using System.Threading.Tasks;
using Asp.Versioning;
using Ecommerce.Model.Discount.Request;
using Ecommerce.Model.Discount.Response;
using Ecommerce.Shared.Infrastructure.RateLimiting;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Order.Application.Commands;
using Order.Application.Queries;

namespace Order.Service.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/discounts")]
    [EnableRateLimiting(RateLimitPolicies.Read)]
    public class DiscountController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DiscountController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("validate")]
        [ProducesResponseType(200, Type = typeof(DiscountValidationResponse))]
        public async Task<IActionResult> Validate([FromBody] ValidateDiscountRequest req)
        {
            var result = await _mediator.Send(new ValidateDiscountQuery(req.CouponCode, req.OrderAmount));
            return Ok(result);
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(200, Type = typeof(List<CouponResponse>))]
        public async Task<IActionResult> GetCoupons()
        {
            var coupons = await _mediator.Send(new GetCouponsQuery());
            return Ok(coupons);
        }

        [HttpPost]
        [Authorize]
        [EnableRateLimiting(RateLimitPolicies.Write)]
        [ProducesResponseType(201, Type = typeof(CouponResponse))]
        public async Task<IActionResult> Create([FromBody] CreateCouponRequest req)
        {
            var coupon = await _mediator.Send(new CreateCouponCommand(req));
            return Created(coupon.Id.ToString(), coupon);
        }

        [HttpPut("{id}")]
        [Authorize]
        [EnableRateLimiting(RateLimitPolicies.Write)]
        [ProducesResponseType(200, Type = typeof(CouponResponse))]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateCouponRequest req)
        {
            var coupon = await _mediator.Send(new UpdateCouponCommand(id, req));
            if (coupon == null) return NotFound();
            return Ok(coupon);
        }
    }
}
