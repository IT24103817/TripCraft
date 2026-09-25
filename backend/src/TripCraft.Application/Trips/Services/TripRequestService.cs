using System.Linq.Expressions;
using System.Text.Json;
using TripCraft.Application.Common;
using TripCraft.Application.Common.Auditing;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Common.Security;
using TripCraft.Application.Trips.Dtos;
using TripCraft.Application.Trips.Planning;

namespace TripCraft.Application.Trips.Services;

public class TripRequestService(
    ITripRequestRepository trips,
    IAuditLogger audit,
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

    /// <summary>Details can only change before planning starts or after a revision request.</summary>
    private static readonly TripRequestStatus[] EditableStatuses =
        [TripRequestStatus.Submitted, TripRequestStatus.RevisionRequested];

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

        var trip = new TripRequest
        {
            TouristId = tourist.Id,
            Objective = request.Objective.Trim(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Pax = request.Pax,
            BudgetUsd = request.BudgetUsd,
            Preferences = PreferencesToJson(request.Preferences),
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
            throw new ConflictException($"Trip request cannot be edited while it is {trip.Status}.");

        var before = TripRequestDto.FromEntity(trip);
        trip.Objective = request.Objective.Trim();
        trip.StartDate = request.StartDate;
        trip.EndDate = request.EndDate;
        trip.Pax = request.Pax;
        trip.BudgetUsd = request.BudgetUsd;
        trip.Preferences = PreferencesToJson(request.Preferences);

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

    /// <summary>404 if the trip does not exist, 403 if a tourist asks for someone else's trip.</summary>
    internal static void EnsureCanAccess(CurrentUser user, TripRequest trip)
    {
        if (user.IsTourist && trip.Tourist?.UserId != user.Id)
            throw new ForbiddenException("You can only access your own trip requests.");
    }

    private async Task<TripRequest> LoadForUserAsync(CurrentUser user, Guid id, CancellationToken ct)
    {
        var trip = await trips.GetByIdAsync(id, ct)
                   ?? throw new NotFoundException("Trip request not found.");
        EnsureCanAccess(user, trip);
        return trip;
    }

    private static string PreferencesToJson(JsonElement? preferences) =>
        preferences?.GetRawText() ?? "{}";
}
