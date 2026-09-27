using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripCraft.Api.Authorization;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Resources.Dtos;
using TripCraft.Application.Resources.Services;

namespace TripCraft.Api.Controllers.Resources;

/// <summary>Component B — hotels and their room types. Operations Manager CRUD (per action); a guide may look up one hotel (voucher QR).</summary>
[ApiController]
[Route("api/hotels")]
[Authorize(Roles = Roles.GuideOrOperationsManager)]
public class HotelsController(IHotelService hotels) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = Roles.OperationsManager)]
    public async Task<ActionResult<PagedResult<HotelDto>>> List([FromQuery] HotelListQuery query, CancellationToken ct) =>
        Ok(await hotels.ListAsync(query, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<HotelDto>> GetById(Guid id, CancellationToken ct) => Ok(await hotels.GetAsync(id, ct));

    [HttpPost]
    [Authorize(Roles = Roles.OperationsManager)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<HotelDto>> Create(SaveHotelRequest request, CancellationToken ct)
    {
        var created = await hotels.CreateAsync(User.GetCurrentUser(), request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.OperationsManager)]
    public async Task<ActionResult<HotelDto>> Update(Guid id, SaveHotelRequest request, CancellationToken ct) =>
        Ok(await hotels.UpdateAsync(User.GetCurrentUser(), id, request, ct));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.OperationsManager)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await hotels.DeleteAsync(User.GetCurrentUser(), id, ct);
        return NoContent();
    }

    /// <summary>The hotel's room types (the same list HotelDto.RoomTypes carries). 404 for an unknown hotel.</summary>
    [HttpGet("{id:guid}/room-types")]
    public async Task<ActionResult<IReadOnlyList<RoomTypeDto>>> ListRoomTypes(Guid id, CancellationToken ct) =>
        Ok((await hotels.GetAsync(id, ct)).RoomTypes);

    [HttpPost("{id:guid}/room-types")]
    [Authorize(Roles = Roles.OperationsManager)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RoomTypeDto>> AddRoomType(Guid id, SaveRoomTypeRequest request, CancellationToken ct)
    {
        var created = await hotels.AddRoomTypeAsync(User.GetCurrentUser(), id, request, ct);
        return CreatedAtAction(nameof(GetById), new { id }, created);
    }

    /// <summary>409 if the new total is below the rooms already held on an upcoming night.</summary>
    [HttpPut("{id:guid}/room-types/{roomTypeId:guid}")]
    [Authorize(Roles = Roles.OperationsManager)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RoomTypeDto>> UpdateRoomType(Guid id, Guid roomTypeId, SaveRoomTypeRequest request,
        CancellationToken ct) =>
        Ok(await hotels.UpdateRoomTypeAsync(User.GetCurrentUser(), id, roomTypeId, request, ct));

    [HttpDelete("{id:guid}/room-types/{roomTypeId:guid}")]
    [Authorize(Roles = Roles.OperationsManager)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteRoomType(Guid id, Guid roomTypeId, CancellationToken ct)
    {
        await hotels.DeleteRoomTypeAsync(User.GetCurrentUser(), id, roomTypeId, ct);
        return NoContent();
    }
}
