using System.Security.Claims;
using Ecommerce.Model.User.Request;
using Ecommerce.Shared.Infrastructure.RateLimiting;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using User.Application.Commands;
using User.Application.Queries;

namespace User.Service.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    [EnableRateLimiting(RateLimitPolicies.Read)]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UserController> _logger;

        public UserController(IMediator mediator, ILogger<UserController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        private Guid GetUserId() =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfile()
        {
            var result = await _mediator.Send(new GetProfileQuery(GetUserId()));
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPut("me")]
        [EnableRateLimiting(RateLimitPolicies.Write)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var result = await _mediator.Send(new UpdateProfileCommand(GetUserId(), request));
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPut("me/password")]
        [EnableRateLimiting(RateLimitPolicies.Write)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var result = await _mediator.Send(new ChangePasswordCommand(GetUserId(), request));
            if (!result)
                return BadRequest(new { message = "Password change failed. Check your current password." });

            return NoContent();
        }
    }
}
