using System.Security.Claims;
using Ecommerce.Model.User.Request;
using Ecommerce.Shared.Infrastructure.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using User.Application.Commands;
using User.Application.Queries;

namespace User.Service.Controllers
{
    [ApiController]
    [Route("api/users/me/addresses")]
    [Authorize]
    [EnableRateLimiting(RateLimitPolicies.Read)]
    public class AddressController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AddressController> _logger;

        public AddressController(IMediator mediator, ILogger<AddressController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        private Guid GetUserId() =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAddresses()
        {
            var result = await _mediator.Send(new GetAddressesQuery(GetUserId()));
            return Ok(result);
        }

        [HttpPost]
        [EnableRateLimiting(RateLimitPolicies.Write)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddAddress([FromBody] AddressRequest request)
        {
            var result = await _mediator.Send(new AddAddressCommand(GetUserId(), request));
            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpPut("{id:long}")]
        [EnableRateLimiting(RateLimitPolicies.Write)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAddress(long id, [FromBody] AddressRequest request)
        {
            var result = await _mediator.Send(new UpdateAddressCommand(GetUserId(), id, request));
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpDelete("{id:long}")]
        [EnableRateLimiting(RateLimitPolicies.Write)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAddress(long id)
        {
            var result = await _mediator.Send(new DeleteAddressCommand(GetUserId(), id));
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}
