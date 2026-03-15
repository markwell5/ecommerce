using System.Threading.Tasks;
using Ecommerce.Model.Stock.Request;
using Ecommerce.Model.Stock.Response;
using Ecommerce.Shared.Infrastructure.RateLimiting;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using Stock.Application.Commands;
using Stock.Application.Queries;

namespace Stock.Service.Controllers
{
    [ApiController]
    [Route("api/stock")]
    [EnableRateLimiting(RateLimitPolicies.Read)]
    public class StockController : ControllerBase
    {
        private readonly ILogger<StockController> _logger;
        private readonly IMediator _mediator;

        public StockController(ILogger<StockController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpGet("{productId}")]
        [ProducesResponseType(200, Type = typeof(StockResponse))]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetStock(long productId)
        {
            var stock = await _mediator.Send(new GetStockQuery(productId));

            if (stock == null)
                return NotFound();

            return Ok(stock);
        }

        [HttpPut("{productId}")]
        [Authorize]
        [EnableRateLimiting(RateLimitPolicies.Write)]
        [ProducesResponseType(200, Type = typeof(StockResponse))]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateStock(long productId, [FromBody] UpdateStockRequest request)
        {
            var stock = await _mediator.Send(new UpdateStockCommand(productId, request.Quantity));

            if (stock == null)
                return NotFound();

            return Ok(stock);
        }
    }
}
