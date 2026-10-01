using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Common;
using TripCraft.Application.Common.Auditing;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Common.Settings;
using TripCraft.Application.Common.Security;
using TripCraft.Application.Resources.Dtos;
using TripCraft.Application.Trips;
using TripCraft.Application.Vouchers;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Application.Resources.Services;

public interface IGuideScheduleService
{
    Task<GuideScheduleDto> GetMyScheduleAsync(CurrentUser user, CancellationToken ct);
    Task<GuideScheduleDto> GetScheduleAsync(Guid guideId, CancellationToken ct);
    Task<CheckInResultDto> CheckInAsync(CurrentUser user, CheckInRequest request, CancellationToken ct);
}

/// <summary>
/// The guide's side of Component B. A guide sees only trips they are held for (resource-based authorisation)
/// and checks in at each stop by scanning the tourist's signed trip voucher, or by GPS within 500 m.
/// Check-in business rule, saved in one SaveChanges (one transaction): the first check-in moves the trip
/// Confirmed → InProgress; the check-in at the last stop moves it to Completed.
/// </summary>
public class GuideScheduleService(
    IResourceRepository resources,
    ITripRequestRepository trips,
    IVoucherRepository vouchers,
    VoucherSigner signer,
    TripSettings settings,
    IAuditLogger audit,
    IUnitOfWork unitOfWork) : IGuideScheduleService
{
    private static readonly TripRequestStatus[] Scheduled =
        [TripRequestStatus.Confirmed, TripRequestStatus.InProgress, TripRequestStatus.Completed];

    public async Task<GuideScheduleDto> GetMyScheduleAsync(CurrentUser user, CancellationToken ct) =>
        await BuildAsync(await GuideOfAsync(user, ct), ct);

    public async Task<GuideScheduleDto> GetScheduleAsync(Guid guideId, CancellationToken ct) =>
        await BuildAsync(await resources.Guides().FirstOrDefaultAsync(g => g.Id == guideId, ct)
                         ?? throw new NotFoundException("Guide not found."), ct);

    public async Task<CheckInResultDto> CheckInAsync(CurrentUser user, CheckInRequest request, CancellationToken ct) =>
        string.IsNullOrWhiteSpace(request.VoucherCode)
            ? await CheckInByGpsAsync(user, request, ct)
            : await CheckInByVoucherAsync(user, request.VoucherCode, request.TripRequestId, ct);

    /// <summary>GPS check-in at a chosen stop, within 500 m of it.</summary>
    private async Task<CheckInResultDto> CheckInByGpsAsync(CurrentUser user, CheckInRequest request, CancellationToken ct)
    {
        var guide = await GuideOfAsync(user, ct);
        var stop = await resources.FindStopAsync(request.ItineraryStopId!.Value, ct)
                   ?? throw new NotFoundException("Stop not found.");
        var trip = await LoadGuidedTripAsync(guide, stop.TripRequestId, ct);
        if (await resources.CheckIns().AnyAsync(c => c.ItineraryStopId == stop.StopId, ct))
            throw new ConflictException($"You have already checked in at {stop.AttractionName}.");

        var distance = AvailabilityRules.DistanceMeters(request.Latitude!.Value, request.Longitude!.Value, stop.Latitude, stop.Longitude);
        if (distance > AvailabilityRules.MaxCheckInDistanceMeters)
            throw new ValidationException([new ValidationFailure("latitude",
                $"You are {distance:N0} m from {stop.AttractionName}; move within {AvailabilityRules.MaxCheckInDistanceMeters} m to check in.")]);

        var checkIn = new StopCheckIn
        {
            ItineraryStopId = stop.StopId, GuideId = guide.Id, Method = CheckInMethod.Gps, Latitude = request.Latitude,
            Longitude = request.Longitude, DistanceMeters = distance, CheckedInAt = DateTime.UtcNow
        };
        return await SaveCheckInAsync(user, trip, checkIn, stop.AttractionName, ct);
    }

    /// <summary>
    /// Voucher check-in (v1.1): the guide scans the tourist's trip voucher. Checks, in order: the HMAC signature,
    /// that it is a trip voucher that exists, the trip (if the guide named one), that this guide holds the trip, its
    /// status, and that today is a day of the trip. Then the next stop of today not checked in yet is checked in.
    /// </summary>
    private async Task<CheckInResultDto> CheckInByVoucherAsync(CurrentUser user, string code, Guid? expectedTripId,
        CancellationToken ct)
    {
        var guide = await GuideOfAsync(user, ct);
        var claims = signer.Verify(code)
                     ?? throw Invalid("This voucher is not valid: its signature does not match.");
        if (claims.Type != VoucherType.Trip)
            throw Invalid("This is a hotel voucher. Scan the tourist's trip voucher.");
        if (!await vouchers.Query().AnyAsync(v => v.Id == claims.VoucherId && v.TripRequestId == claims.TripRequestId, ct))
            throw Invalid("This voucher was not issued by TripCraft.");
        if (expectedTripId is { } expected && expected != claims.TripRequestId)
            throw Invalid("This voucher belongs to another trip.");

        var trip = await LoadGuidedTripAsync(guide, claims.TripRequestId, ct);
        var today = settings.Today();
        if (today < trip.StartDate || today > trip.EndDate)
            throw new ConflictException($"This voucher is for {trip.StartDate:dd MMM} – {trip.EndDate:dd MMM yyyy}; today is {today:dd MMM yyyy}.");

        var dayNumber = today.DayNumber - trip.StartDate.DayNumber + 1;
        var itinerary = await trips.GetItineraryAsync(trip.Id, ct) ?? throw new ConflictException("The trip has no saved itinerary.");
        var stops = itinerary.Days.FirstOrDefault(d => d.DayNumber == dayNumber)?.Stops.OrderBy(s => s.Sequence).ToList() ?? [];
        var stopIds = stops.Select(s => s.Id).ToList();
        var done = await resources.CheckIns().Where(c => stopIds.Contains(c.ItineraryStopId)).Select(c => c.ItineraryStopId).ToListAsync(ct);
        var next = stops.FirstOrDefault(s => !done.Contains(s.Id))
                   ?? throw new ConflictException($"Every stop of day {dayNumber} is already checked in.");

        var checkIn = new StopCheckIn
        {
            ItineraryStopId = next.Id, GuideId = guide.Id, Method = CheckInMethod.Voucher, VoucherId = claims.VoucherId,
            CheckedInAt = DateTime.UtcNow
        };
        return await SaveCheckInAsync(user, trip, checkIn, next.Attraction?.Name ?? "Stop", ct);
    }

    /// <summary>The guide must hold the trip (403) and it must be Confirmed or InProgress (409).</summary>
    private async Task<TripRequest> LoadGuidedTripAsync(Guide guide, Guid tripRequestId, CancellationToken ct)
    {
        if (!await HoldsTripAsync(guide.Id, tripRequestId, ct))
            throw new ForbiddenException("You are not the guide of this trip.");
        var trip = await trips.GetByIdAsync(tripRequestId, ct) ?? throw new NotFoundException("Trip request not found.");
        if (trip.Status is not (TripRequestStatus.Confirmed or TripRequestStatus.InProgress))
            throw new ConflictException($"Check-in is only possible on a confirmed or in-progress trip (it is {TripStatusMachine.Describe(trip.Status)}).");
        return trip;
    }

    /// <summary>
    /// Saves the check-in and moves the trip through TripStatusMachine, in one SaveChanges: the first check-in
    /// Confirmed → InProgress, the check-in at the last stop InProgress → Completed.
    /// </summary>
    private async Task<CheckInResultDto> SaveCheckInAsync(CurrentUser user, TripRequest trip, StopCheckIn checkIn,
        string stopName, CancellationToken ct)
    {
        resources.Add(checkIn);
        audit.Record(user.Id, "StopCheckedIn", nameof(StopCheckIn), checkIn.Id, null,
            new { checkIn.ItineraryStopId, StopName = stopName, Method = checkIn.Method.ToString(), checkIn.DistanceMeters, TripRequestId = trip.Id });

        if (trip.Status == TripRequestStatus.Confirmed)
            TripStatusMachine.Move(trip, TripRequestStatus.InProgress, user.Id, $"First check-in at {stopName}.", audit);
        var checkedIn = await resources.CountCheckInsOfTripAsync(trip.Id, ct) + 1;
        if (checkedIn >= await resources.CountStopsOfTripAsync(trip.Id, ct))
            TripStatusMachine.Move(trip, TripRequestStatus.Completed, user.Id, $"Last stop checked in at {stopName}.", audit);

        await unitOfWork.SaveChangesAsync(ct);
        return new CheckInResultDto(checkIn.ItineraryStopId, checkIn.DistanceMeters, checkIn.CheckedInAt, trip.Status.ToString(),
            checkIn.Method.ToString(), stopName);
    }

    private static ValidationException Invalid(string message) => new([new ValidationFailure("voucherCode", message)]);

    private async Task<Guide> GuideOfAsync(CurrentUser user, CancellationToken ct) =>
        await resources.FindGuideByUserAsync(user.Id, ct)
        ?? throw new ForbiddenException("No guide profile is linked to your account.");

    private Task<bool> HoldsTripAsync(Guid guideId, Guid tripRequestId, CancellationToken ct) =>
        resources.Holds().AnyAsync(h => h.ResourceType == ResourceType.Guide && h.ResourceId == guideId
                                        && h.TripRequestId == tripRequestId && h.Status == HoldStatus.Held, ct);

    /// <summary>The guide's held trips (Confirmed onwards) with days, hotels, vehicle and check-in state.</summary>
    private async Task<GuideScheduleDto> BuildAsync(Guide guide, CancellationToken ct)
    {
        var tripIds = await resources.Holds()
            .Where(h => h.ResourceType == ResourceType.Guide && h.ResourceId == guide.Id && h.Status == HoldStatus.Held
                        && h.TripRequestId != null)
            .Select(h => h.TripRequestId!.Value).Distinct().ToListAsync(ct);
        var heldTrips = await trips.Query().Where(t => tripIds.Contains(t.Id) && Scheduled.Contains(t.Status))
            .OrderBy(t => t.StartDate).ToListAsync(ct);

        var vehicleHolds = await resources.Holds()
            .Where(h => h.ResourceType == ResourceType.Vehicle && h.Status == HoldStatus.Held && h.TripRequestId != null
                        && tripIds.Contains(h.TripRequestId.Value))
            .ToListAsync(ct);
        var vehicleIds = vehicleHolds.Select(h => h.ResourceId).ToList();
        var vehicles = await resources.Vehicles().Where(v => vehicleIds.Contains(v.Id))
            .ToDictionaryAsync(v => v.Id, ct);

        var result = new List<GuideTripDto>();
        foreach (var trip in heldTrips)
        {
            var itinerary = await trips.GetItineraryAsync(trip.Id, ct);
            var days = await DaysAsync(trip, itinerary, ct);
            var vehicleId = vehicleHolds.FirstOrDefault(h => h.TripRequestId == trip.Id)?.ResourceId;
            var vehicle = vehicleId is { } id ? vehicles.GetValueOrDefault(id) : null;
            result.Add(new GuideTripDto(trip.Id, trip.Objective, trip.StartDate, trip.EndDate, trip.Pax,
                trip.Status.ToString(), vehicle?.RegistrationNo, days, vehicle?.Type, vehicle?.Seats));
        }
        return new GuideScheduleDto(guide.Id, guide.Name, result);
    }

    private async Task<List<GuideDayDto>> DaysAsync(TripRequest trip, Itinerary? itinerary, CancellationToken ct)
    {
        if (itinerary is null)
            return [];
        var stopIds = itinerary.Days.SelectMany(d => d.Stops).Select(s => s.Id).ToList();
        var checkIns = await resources.CheckIns().Where(c => stopIds.Contains(c.ItineraryStopId))
            .ToDictionaryAsync(c => c.ItineraryStopId, c => c.CheckedInAt, ct);
        var hotelIds = itinerary.Days.Where(d => d.HotelId != null).Select(d => d.HotelId!.Value).ToList();
        var hotels = await resources.Hotels().Where(h => hotelIds.Contains(h.Id)).ToDictionaryAsync(h => h.Id, h => h.Name, ct);

        return itinerary.Days.OrderBy(d => d.DayNumber).Select(d => new GuideDayDto(
            d.DayNumber, trip.StartDate.AddDays(d.DayNumber - 1), d.City,
            d.HotelId is { } hid ? hotels.GetValueOrDefault(hid) : null,
            d.Stops.OrderBy(s => s.Sequence).Select(s => new GuideStopDto(
                s.Id, s.Sequence, s.Attraction?.Name ?? "Stop", s.Attraction?.Latitude ?? 0, s.Attraction?.Longitude ?? 0,
                checkIns.TryGetValue(s.Id, out var at) ? at : null)).ToList())).ToList();
    }
}
