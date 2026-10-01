using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripCraft.Api.Authorization;
using TripCraft.Application.Trips.Templates;

namespace TripCraft.Api.Controllers.Trips;

/// <summary>Mood packages for the tourist's home screen (v1.1).</summary>
[ApiController]
[Route("api/trip-templates")]
[Authorize(Roles = Roles.TouristOrOperationsManager)]
public class TripTemplatesController(ITripTemplateService templates) : ControllerBase
{
    /// <summary>Active packages with a from-price for two people from today's rates.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TripTemplateDto>>> List(CancellationToken ct) =>
        Ok(await templates.ListAsync(ct));

    /// <summary>One package with its day-by-day itinerary (attractions with positions for the map).</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TripTemplateDto>> Get(Guid id, CancellationToken ct) =>
        Ok(await templates.GetAsync(id, ct));

    /// <summary>"Book as is": creates a trip request from the package and starts planning (202).</summary>
    [HttpPost("{id:guid}/book")]
    [Authorize(Roles = Roles.Tourist)]
    [ProducesResponseType(typeof(BookTemplateResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookTemplateResponse>> Book(Guid id, BookTemplateRequest request, CancellationToken ct)
    {
        var booked = await templates.BookAsync(User.GetCurrentUser(), id, request, ct);
        return Accepted($"/api/trip-requests/{booked.Trip.Id}", booked);
    }
}
