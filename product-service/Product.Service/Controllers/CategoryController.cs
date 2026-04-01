using System.Collections.Generic;
using System.Threading.Tasks;
using Asp.Versioning;
using Ecommerce.Model.Category.Request;
using Ecommerce.Model.Category.Response;
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
    [Route("api/v{version:apiVersion}/categories")]
    [EnableRateLimiting(RateLimitPolicies.Read)]
    public class CategoryController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CategoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(List<CategoryResponse>))]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _mediator.Send(new GetCategoriesQuery());
            return Ok(categories);
        }

        [HttpGet("{slug}/products")]
        [ProducesResponseType(200, Type = typeof(CategoryWithProductsResponse))]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetCategoryProducts(
            string slug,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _mediator.Send(new GetCategoryBySlugQuery(slug, page, pageSize));

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        [EnableRateLimiting(RateLimitPolicies.Write)]
        [ProducesResponseType(201, Type = typeof(CategoryResponse))]
        public async Task<IActionResult> Create([FromBody] CreateCategoryRequest req)
        {
            var category = await _mediator.Send(new CreateCategoryCommand(req));
            return Created(category.Id.ToString(), category);
        }

        [HttpPut("{id}")]
        [Authorize]
        [EnableRateLimiting(RateLimitPolicies.Write)]
        [ProducesResponseType(200, Type = typeof(CategoryResponse))]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateCategoryRequest req)
        {
            var category = await _mediator.Send(new UpdateCategoryCommand(id, req));

            if (category == null)
                return NotFound();

            return Ok(category);
        }

        [HttpDelete("{id}")]
        [Authorize]
        [EnableRateLimiting(RateLimitPolicies.Write)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(long id)
        {
            var deleted = await _mediator.Send(new DeleteCategoryCommand(id));

            if (!deleted)
                return NotFound();

            return NoContent();
        }

        [HttpPost("{categoryId}/products/{productId}")]
        [Authorize]
        [EnableRateLimiting(RateLimitPolicies.Write)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> AssignProduct(long categoryId, long productId)
        {
            await _mediator.Send(new AssignProductCategoryCommand(productId, categoryId));
            return Ok();
        }

        [HttpDelete("{categoryId}/products/{productId}")]
        [Authorize]
        [EnableRateLimiting(RateLimitPolicies.Write)]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RemoveProduct(long categoryId, long productId)
        {
            var removed = await _mediator.Send(new RemoveProductCategoryCommand(productId, categoryId));

            if (!removed)
                return NotFound();

            return NoContent();
        }
    }
}
