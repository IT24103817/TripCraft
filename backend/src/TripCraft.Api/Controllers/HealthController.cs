using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TripCraft.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("health")]
public class HealthController : ControllerBase
{
    /// <summary>Liveness check used by Render and the demo warm-up.</summary>
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "ok", timeUtc = DateTime.UtcNow });
}
