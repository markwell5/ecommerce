using System.Threading.Tasks;
using Ecommerce.Model;
using Ecommerce.Model.Product.Request;
using Ecommerce.Model.Product.Response;
using Ecommerce.Shared.Infrastructure.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Product.Application.Commands;
using Product.Application.Queries;

namespace Product.Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [EnableRateLimiting(RateLimitPolicies.Read)]
    public class ProductController : ControllerBase
    {
        private readonly ILogger<ProductController> _logger;
        private readonly IMediator _mediator;

        public ProductController(ILogger<ProductController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpGet("search")]
        [ProducesResponseType(200, Type = typeof(PagedResponse<ProductResponse>))]
        public async Task<IActionResult> SearchProducts(
            [FromQuery] string q = null,
            [FromQuery] string category = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] string sortBy = null,
            [FromQuery] string sortDirection = "asc",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var products = await _mediator.Send(new SearchProductsQuery
            {
                Query = q,
                Category = category,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SortBy = sortBy,
                SortDirection = sortDirection,
                Page = page,
                PageSize = pageSize
            });

            return Ok(products);
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(PagedResponse<ProductResponse>))]
        public async Task<IActionResult> GetProducts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string sortBy = null,
            [FromQuery] string sortDirection = "asc",
            [FromQuery] string search = null)
        {
            var products = await _mediator.Send(new GetProductsQuery
            {
                Page = page,
                PageSize = pageSize,
                SortBy = sortBy,
                SortDirection = sortDirection,
                Search = search
            });

            return Ok(products);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(ProductResponse))]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProduct(long id)
        {
            var product = await _mediator.Send(new GetProductQuery(id));

            if (product == null)
                return NotFound();

            return Ok(product);
        }

        [HttpPost]
        [EnableRateLimiting(RateLimitPolicies.Write)]
        [ProducesResponseType(201, Type = typeof(ProductResponse))]
        public async Task<IActionResult> Create([FromBody] CreateProductRequest req)
        {
            var product = await _mediator.Send(new CreateProductCommand(req));

            return Created(product.Id.ToString(), product);
        }

        [HttpPut("{id}")]
        [EnableRateLimiting(RateLimitPolicies.Write)]
        [ProducesResponseType(200, Type = typeof(ProductResponse))]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateProductRequest req)
        {
            var product = await _mediator.Send(new UpdateProductCommand(id, req));

            if (product == null)
                return NotFound();

            return Ok(product);
        }

        [HttpDelete("{id}")]
        [EnableRateLimiting(RateLimitPolicies.Write)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(long id)
        {
            var deleted = await _mediator.Send(new DeleteProductCommand(id));

            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}
