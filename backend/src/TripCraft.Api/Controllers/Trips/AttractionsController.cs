using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripCraft.Api.Authorization;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Trips.Dtos;
using TripCraft.Application.Trips.Services;

namespace TripCraft.Api.Controllers.Trips;

/// <summary>
/// Attraction catalogue. Any signed-in user can read it; only Operations Managers change it.
/// </summary>
[ApiController]
[Route("api/attractions")]
public class AttractionsController(IAttractionService attractions) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<AttractionDto>>> List([FromQuery] AttractionListQuery query, CancellationToken ct)
    {
        return Ok(await attractions.ListAsync(query, ct));
    }

    /// <summary>Supported destinations for the trip-request city multi-select (v1.1). Free-text cities are rejected.</summary>
    [HttpGet("cities")]
    public async Task<ActionResult<IReadOnlyList<string>>> Cities(CancellationToken ct)
    {
        return Ok(await attractions.ListCitiesAsync(ct));
    }

    /// <summary>Not in PLAN.md's list, but needed so POST can return a Location header.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AttractionDto>> GetById(Guid id, CancellationToken ct)
    {
        return Ok(await attractions.GetAsync(id, ct));
    }

    [HttpPost]
    [Authorize(Roles = Roles.OperationsManager)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AttractionDto>> Create(SaveAttractionRequest request, CancellationToken ct)
    {
        var created = await attractions.CreateAsync(User.GetCurrentUser(), request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [Authorize(Roles = Roles.OperationsManager)]
    public async Task<ActionResult<AttractionDto>> Update(Guid id, SaveAttractionRequest request, CancellationToken ct)
    {
        return Ok(await attractions.UpdateAsync(User.GetCurrentUser(), id, request, ct));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.OperationsManager)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await attractions.DeleteAsync(User.GetCurrentUser(), id, ct);
        return NoContent();
    }
}
