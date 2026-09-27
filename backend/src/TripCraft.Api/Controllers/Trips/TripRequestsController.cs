using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripCraft.Api.Authorization;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Trips.Dtos;
using TripCraft.Application.Trips.Services;
using TripCraft.Application.Workflows.Dtos;
using TripCraft.Application.Workflows.Services;

namespace TripCraft.Api.Controllers.Trips;

/// <summary>
/// Component A — Trip Requests &amp; Itinerary. Tourists submit and see only their own trips
/// (checked in the service); Operations Managers see and manage all of them. Guides and Admins get 403.
/// </summary>
[ApiController]
[Route("api/trip-requests")]
[Authorize(Roles = Roles.TouristOrOperationsManager)]
public class TripRequestsController(
    ITripRequestService tripRequests,
    ITripPlanningService planning,
    IPassportPhotoService passportPhotos,
    IWorkflowQueryService workflows) : ControllerBase
{
    /// <summary>Tourist submits a trip request (PLAN.md section 6, step 1–2).</summary>
    [HttpPost]
    [Authorize(Roles = Roles.Tourist)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<TripRequestDto>> Create(CreateTripRequestRequest request, CancellationToken ct)
    {
        var created = await tripRequests.CreateAsync(User.GetCurrentUser(), request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<TripRequestDto>>> List([FromQuery] TripRequestListQuery query, CancellationToken ct)
    {
        return Ok(await tripRequests.ListAsync(User.GetCurrentUser(), query, ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TripRequestDto>> GetById(Guid id, CancellationToken ct)
    {
        return Ok(await tripRequests.GetAsync(User.GetCurrentUser(), id, ct));
    }

    /// <summary>Edit details. 409 unless the request is Submitted or RevisionRequested.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TripRequestDto>> Update(Guid id, UpdateTripRequestRequest request, CancellationToken ct)
    {
        return Ok(await tripRequests.UpdateAsync(User.GetCurrentUser(), id, request, ct));
    }

    /// <summary>
    /// Business operation: validates passport/dates, builds the day-by-day skeleton, creates the
    /// agent workflow and hands it to the agent service. 202 because planning continues in the background.
    /// Tourist owner only (checked in the service); 409 if a workflow is already running.
    /// </summary>
    [HttpPost("{id:guid}/start-planning")]
    [Authorize(Roles = Roles.Tourist)]
    [ProducesResponseType(typeof(StartPlanningResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StartPlanningResponse>> StartPlanning(Guid id, CancellationToken ct)
    {
        var result = await planning.StartPlanningAsync(User.GetCurrentUser(), id, ct);
        return Accepted($"/api/workflows/{result.WorkflowId}", result);
    }

    /// <summary>Status workflow: Submitted → Cancelled (owner Tourist or a manager). 409 in any other status.</summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TripRequestDto>> Cancel(Guid id, CancellationToken ct)
    {
        return Ok(await tripRequests.CancelAsync(User.GetCurrentUser(), id, ct));
    }

    /// <summary>History: every audited change of the trip and its agent workflows, oldest first.</summary>
    [HttpGet("{id:guid}/history")]
    public async Task<ActionResult<IReadOnlyList<TripHistoryEntryDto>>> GetHistory(Guid id, CancellationToken ct)
    {
        return Ok(await tripRequests.GetHistoryAsync(User.GetCurrentUser(), id, ct));
    }

    [HttpGet("{id:guid}/itinerary")]
    public async Task<ActionResult<ItineraryDto>> GetItinerary(Guid id, CancellationToken ct)
    {
        return Ok(await tripRequests.GetItineraryAsync(User.GetCurrentUser(), id, ct));
    }

    /// <summary>
    /// Itinerary editor: an Operations Manager changes one day's stops (1–3 attractions in the day's city) and
    /// notes while the trip is Confirmed. 409 in any other status or without a saved itinerary.
    /// </summary>
    [HttpPut("{id:guid}/itinerary/days/{dayNumber:int}")]
    [Authorize(Roles = Roles.OperationsManager)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ItineraryDto>> EditItineraryDay(Guid id, int dayNumber,
        EditItineraryDayRequest request, [FromServices] IItineraryEditService editor, CancellationToken ct)
    {
        return Ok(await editor.EditDayAsync(User.GetCurrentUser().Id, id, dayNumber, request, ct));
    }

    /// <summary>
    /// Passport photo from the Flutter camera (multipart field "file"). Owner Tourist only; JPEG/PNG up to 5 MB,
    /// checked by content, stored privately under a random name (PLAN.md section 10).
    /// </summary>
    [HttpPost("{id:guid}/passport-photo")]
    [Authorize(Roles = Roles.Tourist)]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(PassportPhotoService.MaxBytes + 64 * 1024)]
    public async Task<ActionResult<PassportPhotoResponse>> UploadPassportPhoto(Guid id, IFormFile file, CancellationToken ct)
    {
        await using var stream = file.OpenReadStream();
        return Ok(await passportPhotos.UploadAsync(User.GetCurrentUser(), id, stream, file.ContentType, file.Length, ct));
    }

    /// <summary>The trip's newest agent workflow (status, itinerary proposal, quotation). 404 before planning starts.</summary>
    [HttpGet("{id:guid}/workflow")]
    public async Task<ActionResult<WorkflowDto>> GetWorkflow(Guid id, CancellationToken ct)
    {
        return Ok(await workflows.GetLatestForTripAsync(User.GetCurrentUser(), id, ct));
    }
}
