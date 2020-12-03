using Ecommerce.Model.Product.Request;
using Ecommerce.Model.Product.Response;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Product.Application.Commands;
using Product.Application.Queries;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Product.Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ILogger<ProductController> _logger;
        private readonly IMediator _mediator;

        public ProductController(ILogger<ProductController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<ProductResponse>))]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _mediator.Send(new GetProductsQuery());
            
            return Ok(products);
        }

        [HttpGet("{key}")]
        [ProducesResponseType(200, Type = typeof(ProductResponse))]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProduct(long key)
        {
            var product = await _mediator.Send(new GetProductQuery(key));

            if (product == null)
                return NotFound();

            return Ok(product);
        }

        [HttpPost]
        [ProducesResponseType(201, Type = typeof(ProductResponse))]
        public async Task<IActionResult> Create([FromBody] CreateProductRequest req)
        {
            var product = await _mediator.Send(new CreateProductCommand(req));

            return Created(product.Key.ToString(), product);
        }
    }
}
