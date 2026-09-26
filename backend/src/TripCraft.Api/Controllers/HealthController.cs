using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripCraft.Application.Common;

namespace TripCraft.Api.Controllers;

/// <summary>{status, version, db} for Render's health check and the demo warm-up.</summary>
public record HealthResponse(string Status, string Version, string Db, DateTime TimeUtc);

[ApiController]
[AllowAnonymous]
[Route("health")]
public class HealthController(IDatabaseHealth database) : ControllerBase
{
    private static readonly TimeSpan DbTimeout = TimeSpan.FromSeconds(3);

    /// <summary>200 when the database answers; 503 ("degraded", db "fail") when it does not.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<HealthResponse>> Get(CancellationToken ct)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(DbTimeout);
        var dbOk = await database.CanConnectAsync(timeout.Token);

        var body = new HealthResponse(dbOk ? "ok" : "degraded", AppVersion, dbOk ? "ok" : "fail", DateTime.UtcNow);
        return dbOk ? Ok(body) : StatusCode(StatusCodes.Status503ServiceUnavailable, body);
    }

    /// <summary>Assembly version plus the short git commit Render injects (RENDER_GIT_COMMIT), e.g. "1.0.0+3f2c1ab".</summary>
    private static readonly string AppVersion = BuildVersion();

    private static string BuildVersion()
    {
        var version = typeof(HealthController).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion.Split('+')[0] ?? "0.0.0";
        var commit = Environment.GetEnvironmentVariable("RENDER_GIT_COMMIT");
        return string.IsNullOrWhiteSpace(commit) ? version : $"{version}+{commit[..Math.Min(7, commit.Length)]}";
    }
}
