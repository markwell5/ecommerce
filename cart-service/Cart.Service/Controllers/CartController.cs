using Cart.Application.Commands;
using Cart.Application.DTOs;
using Cart.Application.Queries;
using Ecommerce.Shared.Infrastructure.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Cart.Service.Controllers;

[ApiController]
[Route("[controller]")]
[EnableRateLimiting(RateLimitPolicies.Read)]
public class CartController : ControllerBase
{
    private readonly IMediator _mediator;

    public CartController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{cartId}")]
    [ProducesResponseType(200, Type = typeof(CartDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetCart(string cartId)
    {
        var cart = await _mediator.Send(new GetCartQuery(cartId));
        return cart is null ? NotFound() : Ok(cart);
    }

    [HttpPost("{cartId}/items")]
    [EnableRateLimiting(RateLimitPolicies.Write)]
    [ProducesResponseType(200, Type = typeof(CartDto))]
    public async Task<IActionResult> AddToCart(string cartId, [FromBody] AddToCartRequest request)
    {
        var cart = await _mediator.Send(new AddToCartCommand(cartId, request.ProductId, request.Quantity));
        return Ok(cart);
    }

    [HttpPut("{cartId}/items/{productId}")]
    [EnableRateLimiting(RateLimitPolicies.Write)]
    [ProducesResponseType(200, Type = typeof(CartDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateQuantity(string cartId, long productId, [FromBody] UpdateQuantityRequest request)
    {
        var cart = await _mediator.Send(new UpdateQuantityCommand(cartId, productId, request.Quantity));
        return cart is null ? NotFound() : Ok(cart);
    }

    [HttpDelete("{cartId}/items/{productId}")]
    [EnableRateLimiting(RateLimitPolicies.Write)]
    [ProducesResponseType(200, Type = typeof(CartDto))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RemoveFromCart(string cartId, long productId)
    {
        var cart = await _mediator.Send(new RemoveFromCartCommand(cartId, productId));
        return cart is null ? NotFound() : Ok(cart);
    }

    [HttpDelete("{cartId}")]
    [EnableRateLimiting(RateLimitPolicies.Write)]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ClearCart(string cartId)
    {
        var cleared = await _mediator.Send(new ClearCartCommand(cartId));
        return cleared ? NoContent() : NotFound();
    }
}

public record AddToCartRequest(long ProductId, int Quantity);
public record UpdateQuantityRequest(int Quantity);
