using System;
using System.Threading.Tasks;
using Ecommerce.Model.Order.Request;
using Ecommerce.Model.Order.Response;
using Ecommerce.Shared.Infrastructure.RateLimiting;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using Order.Application.Commands;
using Order.Application.Queries;

namespace Order.Service.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [EnableRateLimiting(RateLimitPolicies.Read)]
    public class OrderController : ControllerBase
    {
        private readonly ILogger<OrderController> _logger;
        private readonly IMediator _mediator;

        public OrderController(ILogger<OrderController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpPost]
        [EnableRateLimiting(RateLimitPolicies.Write)]
        [ProducesResponseType(202, Type = typeof(OrderResponse))]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request)
        {
            var order = await _mediator.Send(new PlaceOrderCommand(request));
            return Accepted(order);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(OrderResponse))]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetOrder(Guid id)
        {
            var order = await _mediator.Send(new GetOrderQuery(id));

            if (order == null)
                return NotFound();

            return Ok(order);
        }

        [HttpPost("{id}/cancel")]
        [EnableRateLimiting(RateLimitPolicies.Write)]
        [ProducesResponseType(202)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> CancelOrder(Guid id)
        {
            var result = await _mediator.Send(new CancelOrderCommand(id));
            return result ? Accepted() : Conflict();
        }

        [HttpPost("{id}/ship")]
        [EnableRateLimiting(RateLimitPolicies.Write)]
        [ProducesResponseType(202)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> ShipOrder(Guid id)
        {
            var result = await _mediator.Send(new ShipOrderCommand(id));
            return result ? Accepted() : Conflict();
        }

        [HttpPost("{id}/deliver")]
        [EnableRateLimiting(RateLimitPolicies.Write)]
        [ProducesResponseType(202)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> DeliverOrder(Guid id)
        {
            var result = await _mediator.Send(new DeliverOrderCommand(id));
            return result ? Accepted() : Conflict();
        }

        [HttpPost("{id}/return")]
        [EnableRateLimiting(RateLimitPolicies.Write)]
        [ProducesResponseType(202)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> ReturnOrder(Guid id)
        {
            var result = await _mediator.Send(new ReturnOrderCommand(id));
            return result ? Accepted() : Conflict();
        }
    }
}
