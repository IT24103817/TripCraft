using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TripCraft.Api.Authorization;
using TripCraft.Api.Setup;
using TripCraft.Application.Identity.Dtos;
using TripCraft.Application.Identity.Services;

namespace TripCraft.Api.Controllers.Identity;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>Self-registration. Always creates a Tourist account.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [AllowAnonymous]
    public async Task<ActionResult<UserDto>> Register(RegisterRequest request, CancellationToken ct)
    {
        var user = await authService.RegisterAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, user);
    }

    /// <summary>Returns a 60-minute JWT. Limited to 5 attempts per minute per IP.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingSetup.LoginPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        return Ok(await authService.LoginAsync(request, ct));
    }

    /// <summary>Replaces the signed-in user's password and clears must-change-password (a guide's first login).</summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserDto>> ChangePassword(ChangePasswordRequest request, CancellationToken ct)
    {
        return Ok(await authService.ChangePasswordAsync(User.GetUserId(), request, ct));
    }

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me(CancellationToken ct)
    {
        return Ok(await authService.GetCurrentUserAsync(User.GetUserId(), ct));
    }
}
