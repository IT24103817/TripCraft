using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripCraft.Api.Authorization;
using TripCraft.Application.Common.Settings;

namespace TripCraft.Api.Controllers.Admin;

/// <summary>Operator settings (v1.1): LLM provider, cancellation notice, margin and deposit. Admin only.</summary>
[ApiController]
[Route("api/admin/settings")]
[Authorize(Roles = Roles.Admin)]
public class AdminSettingsController(ISettingsService settings) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<SettingsDto>> Get(CancellationToken ct) => Ok(await settings.GetAsync(ct));

    /// <summary>Takes effect on the next request; a margin change writes today's rate card.</summary>
    [HttpPut]
    public async Task<ActionResult<SettingsDto>> Save(SaveSettingsRequest request, CancellationToken ct) =>
        Ok(await settings.SaveAsync(User.GetCurrentUser(), request, ct));
}
