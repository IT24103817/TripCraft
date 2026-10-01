using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripCraft.Api.Authorization;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Resources.Dtos;
using TripCraft.Application.Resources.Services;

namespace TripCraft.Api.Controllers.Resources;

/// <summary>Component B business operations: availability search, the hold calendar, manual blocks and releases.</summary>
[ApiController]
[Authorize(Roles = Roles.OperationsManager)]
public class AvailabilityController(IAvailabilityService availability, IResourceHoldAdminService holds,
    IAvailabilityGridService grid) : ControllerBase
{
    /// <summary>Resource-by-day grid (v1.1): Free / Held / Confirmed / Blocked per guide, vehicle and room type. Max 62 days.</summary>
    [HttpGet("api/availability/grid")]
    public async Task<ActionResult<AvailabilityGridDto>> Grid([FromQuery] AvailabilityGridQuery query, CancellationToken ct) =>
        Ok(await grid.GetAsync(query, ct));

    [HttpGet("api/resource-holds/{id:guid}")]
    public async Task<ActionResult<HoldDto>> GetHold(Guid id, CancellationToken ct) => Ok(await holds.GetAsync(id, ct));

    /// <summary>Edit a manual block (dates, quantity, note). 409 for a trip's hold or when the new dates overlap.</summary>
    [HttpPut("api/resource-holds/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<HoldDto>> UpdateBlock(Guid id, UpdateBlockRequest request, CancellationToken ct) =>
        Ok(await holds.UpdateBlockAsync(User.GetCurrentUser(), id, request, ct));

    /// <summary>Free guides (language, pax), vehicles (seats) or rooms (city, rooms on every night) for a date range.</summary>
    [HttpGet("api/availability")]
    public async Task<ActionResult<IReadOnlyList<AvailableResourceDto>>> Search([FromQuery] AvailabilityQuery query,
        CancellationToken ct) =>
        Ok(await availability.SearchAsync(query, ct));

    /// <summary>Holds overlapping a date range, with resource names (the availability calendar).</summary>
    [HttpGet("api/resource-holds")]
    public async Task<ActionResult<PagedResult<HoldDto>>> ListHolds([FromQuery] HoldListQuery query, CancellationToken ct) =>
        Ok(await holds.ListAsync(query, ct));

    /// <summary>Manual block (e.g. vehicle maintenance). Same overlap rules as an approval: 409 on conflict.</summary>
    [HttpPost("api/resource-holds")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<HoldDto>> CreateHold(CreateHoldRequest request, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, await holds.CreateAsync(User.GetCurrentUser(), request, ct));

    [HttpPost("api/resource-holds/{id:guid}/release")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<HoldDto>> Release(Guid id, CancellationToken ct) =>
        Ok(await holds.ReleaseAsync(User.GetCurrentUser(), id, ct));
}
