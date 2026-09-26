using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripCraft.Api.Authorization;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Resources.Dtos;
using TripCraft.Application.Resources.Services;

namespace TripCraft.Api.Controllers.Resources;

/// <summary>Component B — vehicles. Operations Manager CRUD (per action); a guide may look up one vehicle.</summary>
[ApiController]
[Route("api/vehicles")]
[Authorize(Roles = Roles.GuideOrOperationsManager)]
public class VehiclesController(IVehicleService vehicles) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = Roles.OperationsManager)]
    public async Task<ActionResult<PagedResult<VehicleDto>>> List([FromQuery] VehicleListQuery query, CancellationToken ct) =>
        Ok(await vehicles.ListAsync(query, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VehicleDto>> GetById(Guid id, CancellationToken ct) => Ok(await vehicles.GetAsync(id, ct));

    [HttpPost]
    [Authorize(Roles = Roles.OperationsManager)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VehicleDto>> Create(SaveVehicleRequest request, CancellationToken ct)
    {
        var created = await vehicles.CreateAsync(User.GetCurrentUser(), request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.OperationsManager)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VehicleDto>> Update(Guid id, SaveVehicleRequest request, CancellationToken ct) =>
        Ok(await vehicles.UpdateAsync(User.GetCurrentUser(), id, request, ct));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.OperationsManager)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await vehicles.DeleteAsync(User.GetCurrentUser(), id, ct);
        return NoContent();
    }
}
