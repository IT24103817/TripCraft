using System.Linq.Expressions;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Common;
using TripCraft.Application.Common.Auditing;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Common.Notifications;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Common.Security;
using TripCraft.Application.Common.Settings;
using TripCraft.Application.Trips.Dtos;
using TripCraft.Application.Trips.Planning;
using TripCraft.Application.Workflows;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Application.Trips.Services;

public class TripRequestService(
    ITripRequestRepository trips,
    IAttractionRepository attractions,
    IAgentWorkflowRepository workflows,
    IResourceHoldService holds,
    INotifier notifier,
    TripSettings settings,
    IAuditLogger audit,
    IAuditLogReader auditLogs,
    IUnitOfWork unitOfWork) : ITripRequestService
{
    /// <summary>Whitelist for ?sort=. Anything else is rejected by the validator with 400.</summary>
    public static readonly IReadOnlyDictionary<string, Expression<Func<TripRequest, object>>> SortableFields =
        new Dictionary<string, Expression<Func<TripRequest, object>>>
        {
            ["createdAt"] = t => t.CreatedAt,
            ["startDate"] = t => t.StartDate,
            ["budgetUsd"] = t => t.BudgetUsd,
            ["pax"] = t => t.Pax,
            ["status"] = t => t.Status
        };

    /// <summary>Details can only change before planning starts or after planning failed safely.</summary>
    private static readonly TripRequestStatus[] EditableStatuses =
        [TripRequestStatus.Submitted, TripRequestStatus.FailedSafely];

    public async Task<TripRequestDto> CreateAsync(CurrentUser user, CreateTripRequestRequest request, CancellationToken ct)
    {
        var tourist = await trips.GetTouristByUserIdAsync(user.Id, ct);
        if (tourist is null)
        {
            tourist = new Tourist { UserId = user.Id };
            trips.AddTourist(tourist);
        }
        tourist.Nationality = request.Nationality.Trim();
        tourist.PassportNumberMasked = TripPlanningRules.MaskPassport(request.PassportNumber);
        var cities = await SupportedCitiesAsync(request.Cities, ct);

        var trip = new TripRequest
        {
            TouristId = tourist.Id,
            Objective = request.Objective.Trim(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Pax = request.Pax,
            BudgetUsd = request.BudgetUsd,
            Preferences = PreferencesToJson(request.Preferences),
            Cities = TripRequest.JoinCities(cities),
            Status = TripRequestStatus.Submitted
        };
        trips.Add(trip);

        audit.Record(user.Id, "TripRequestCreated", nameof(TripRequest), trip.Id, null, TripRequestDto.FromEntity(trip));
        await unitOfWork.SaveChangesAsync(ct);

        return TripRequestDto.FromEntity(trip);
    }

    public async Task<PagedResult<TripRequestDto>> ListAsync(CurrentUser user, TripRequestListQuery query, CancellationToken ct)
    {
        var q = trips.Query();

        // Resource-based rule: tourists only ever see their own trips.
        if (user.IsTourist)
            q = q.Where(t => t.Tourist!.UserId == user.Id);

        if (query.Status.HasValue)
            q = q.Where(t => t.Status == query.Status.Value);
        if (query.From.HasValue)
            q = q.Where(t => t.StartDate >= query.From.Value);
        if (query.To.HasValue)
            q = q.Where(t => t.StartDate <= query.To.Value);
        foreach (var city in query.Cities ?? [])
        {
            // Every chosen city must be on the trip (cities are stored as "|Kandy|Ella|").
            var token = "|" + city.Trim().ToLower() + "|";
            q = q.Where(t => t.Cities.ToLower().Contains(token));
        }
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            q = q.Where(t => t.Objective.ToLower().Contains(term));
        }

        return await q
            .ApplySort(query.Sort, SortableFields, "-createdAt")
            .ToPagedResultAsync(query.Page, query.PageSize, TripRequestDto.FromEntity, ct);
    }

    public async Task<TripRequestDto> GetAsync(CurrentUser user, Guid id, CancellationToken ct)
    {
        var trip = await LoadForUserAsync(user, id, ct);
        return TripRequestDto.FromEntity(trip);
    }

    public async Task<TripRequestDto> UpdateAsync(CurrentUser user, Guid id, UpdateTripRequestRequest request, CancellationToken ct)
    {
        var trip = await LoadForUserAsync(user, id, ct);

        if (!EditableStatuses.Contains(trip.Status))
            throw new ConflictException($"Trip request cannot be edited while it is {TripStatusMachine.Describe(trip.Status)}.");
        var cities = await SupportedCitiesAsync(request.Cities, ct);

        var before = TripRequestDto.FromEntity(trip);
        trip.Objective = request.Objective.Trim();
        trip.StartDate = request.StartDate;
        trip.EndDate = request.EndDate;
        trip.Pax = request.Pax;
        trip.BudgetUsd = request.BudgetUsd;
        trip.Preferences = PreferencesToJson(request.Preferences);
        trip.Cities = TripRequest.JoinCities(cities);

        audit.Record(user.Id, "TripRequestUpdated", nameof(TripRequest), trip.Id, before, TripRequestDto.FromEntity(trip));
        await unitOfWork.SaveChangesAsync(ct);

        return TripRequestDto.FromEntity(trip);
    }

    public async Task<ItineraryDto> GetItineraryAsync(CurrentUser user, Guid id, CancellationToken ct)
    {
        await LoadForUserAsync(user, id, ct);
        var itinerary = await trips.GetItineraryAsync(id, ct)
                        ?? throw new NotFoundException("This trip request has no itinerary yet.");
        return ItineraryDto.FromEntity(itinerary);
    }

    public async Task<IReadOnlyList<TripHistoryEntryDto>> GetHistoryAsync(CurrentUser user, Guid id, CancellationToken ct)
    {
        await LoadForUserAsync(user, id, ct);

        // The trip's own rows plus the rows of every agent workflow run for it.
        var workflowIds = await workflows.Query().Where(w => w.TripRequestId == id).Select(w => w.Id).ToListAsync(ct);
        var entityIds = workflowIds.Append(id).ToList();

        var rows = await auditLogs.Query()
            .Where(a => entityIds.Contains(a.EntityId))
            .OrderBy(a => a.At)
            .ToListAsync(ct);
        return rows.Select(TripHistoryEntryDto.FromAudit).ToList();
    }

    /// <summary>
    /// Cancels with a reason (v1.1). The tourist may cancel until the cut-off (CANCELLATION_CUTOFF_DAYS before the
    /// start); a manager at any time. Statuses come from TripStatusMachine (409 e.g. while the agents are planning).
    /// Held resources are released in the same transaction, and the other side and the guide are notified.
    /// </summary>
    public async Task<TripRequestDto> CancelAsync(CurrentUser user, Guid id, string reason, CancellationToken ct)
    {
        var trip = await LoadForUserAsync(user, id, ct);
        if (ClosedReason(user, trip) is { } closed)
            throw new ConflictException(closed);

        var workflow = await workflows.GetLatestForTripAsync(trip.Id, ct);
        var guideId = ProposalValidator.ParseId(
            WorkflowJson.Deserialize<WorkflowOutcome>(workflow?.FinalOutcome)?.Proposal.Resources?.GuideId);

        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        var released = await holds.ReleaseTripHoldsAsync(trip.Id, null, ct);
        var by = user.IsTourist ? "the client" : "the operator";
        TripStatusMachine.Move(trip, TripRequestStatus.Cancelled, user.Id, $"Cancelled by {by}: {reason.Trim()}", audit);
        if (workflow is { Status: not (AgentWorkflowStatus.Completed or AgentWorkflowStatus.FailedSafely or AgentWorkflowStatus.Rejected) })
        {
            workflow.Status = AgentWorkflowStatus.Rejected;
            workflow.CurrentStep = "cancelled";
            workflow.FinishedAt = DateTime.UtcNow;
        }

        if (user.IsTourist)
            await notifier.NotifyManagersAsync("TripCancelled", "A client cancelled a trip",
                $"{trip.StartDate:dd MMM yyyy}, {trip.Pax} people: {reason.Trim()}", trip.Id, ct);
        else
            notifier.NotifyTourist(trip, "TripCancelled", "Your trip was cancelled", reason.Trim());
        if (released > 0 && guideId is { } guide)
            await notifier.NotifyGuideAsync(guide, "TripCancelled", "A trip you were assigned to was cancelled",
                $"{trip.StartDate:dd MMM} – {trip.EndDate:dd MMM}.", trip.Id, ct);
        audit.Record(user.Id, "TripRequestCancelled", nameof(TripRequest), trip.Id, null,
            new { Reason = reason.Trim(), HoldsReleased = released });

        await unitOfWork.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return TripRequestDto.FromEntity(trip);
    }

    public async Task<CancellationInfoDto> GetCancellationInfoAsync(CurrentUser user, Guid id, CancellationToken ct)
    {
        var trip = await LoadForUserAsync(user, id, ct);
        var closed = ClosedReason(user, trip);
        return new CancellationInfoDto(closed is null, settings.CancelUntil(trip.StartDate), settings.CancellationCutoffDays,
            closed, settings.OperatorContact);
    }

    /// <summary>Why this caller cannot cancel the trip now, or null when they can.</summary>
    private string? ClosedReason(CurrentUser user, TripRequest trip)
    {
        if (!TripStatusMachine.CanMove(trip.Status, TripRequestStatus.Cancelled))
            return $"A trip that is {TripStatusMachine.Describe(trip.Status)} cannot be cancelled.";
        var cancelUntil = settings.CancelUntil(trip.StartDate);
        if (user.IsTourist && settings.Today() > cancelUntil)
            return $"Cancellation closed on {cancelUntil:dd MMM yyyy}, {settings.CancellationCutoffDays} days before the start. " +
                   $"Please contact the operator: {settings.OperatorContact}.";
        return null;
    }

    /// <summary>
    /// The chosen cities, spelled as in the attractions list. A city we do not cover is a 400 that names the
    /// supported cities (free text is not accepted).
    /// </summary>
    private async Task<List<string>> SupportedCitiesAsync(IReadOnlyList<string>? requested, CancellationToken ct)
    {
        var known = await attractions.ListActiveCitiesAsync(ct);
        var result = new List<string>();
        foreach (var city in requested ?? [])
        {
            var match = known.FirstOrDefault(k => string.Equals(k, city.Trim(), StringComparison.OrdinalIgnoreCase))
                        ?? throw new ValidationException([new ValidationFailure("cities",
                            $"'{city.Trim()}' is not a city we cover. Supported cities: {string.Join(", ", known)}.")]);
            result.Add(match);
        }
        return result;
    }

    private async Task<TripRequest> LoadForUserAsync(CurrentUser user, Guid id, CancellationToken ct)
    {
        var trip = await trips.GetByIdAsync(id, ct)
                   ?? throw new NotFoundException("Trip request not found.");
        TripAccess.EnsureCanAccess(user, trip.Tourist?.UserId);
        return trip;
    }

    private static string PreferencesToJson(JsonElement? preferences) =>
        preferences?.GetRawText() ?? "{}";
}
