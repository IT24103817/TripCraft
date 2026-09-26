using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Common;
using TripCraft.Application.Common.Auditing;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Common.Security;
using TripCraft.Application.Resources.Dtos;
using TripCraft.Application.Trips;
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
/// and checks in at each stop within 500 m. Check-in business rule, saved in one SaveChanges (one transaction):
/// the first check-in moves the trip Confirmed → InProgress; the check-in at the last stop moves it to Completed.
/// </summary>
public class GuideScheduleService(
    IResourceRepository resources,
    ITripRequestRepository trips,
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

    public async Task<CheckInResultDto> CheckInAsync(CurrentUser user, CheckInRequest request, CancellationToken ct)
    {
        var guide = await GuideOfAsync(user, ct);
        var stop = await resources.FindStopAsync(request.ItineraryStopId, ct)
                   ?? throw new NotFoundException("Stop not found.");
        if (!await HoldsTripAsync(guide.Id, stop.TripRequestId, ct))
            throw new ForbiddenException("You are not the guide of this trip.");

        var trip = await trips.GetByIdAsync(stop.TripRequestId, ct) ?? throw new NotFoundException("Trip request not found.");
        if (trip.Status is not (TripRequestStatus.Confirmed or TripRequestStatus.InProgress))
            throw new ConflictException($"Check-in is only possible on a Confirmed or InProgress trip (it is {trip.Status}).");
        if (await resources.CheckIns().AnyAsync(c => c.ItineraryStopId == stop.StopId, ct))
            throw new ConflictException($"You have already checked in at {stop.AttractionName}.");

        var distance = AvailabilityRules.DistanceMeters(request.Latitude, request.Longitude, stop.Latitude, stop.Longitude);
        if (distance > AvailabilityRules.MaxCheckInDistanceMeters)
            throw new ValidationException([new ValidationFailure("latitude",
                $"You are {distance:N0} m from {stop.AttractionName}; move within {AvailabilityRules.MaxCheckInDistanceMeters} m to check in.")]);

        var checkIn = new StopCheckIn
        {
            ItineraryStopId = stop.StopId, GuideId = guide.Id, Latitude = request.Latitude,
            Longitude = request.Longitude, DistanceMeters = distance, CheckedInAt = DateTime.UtcNow
        };
        resources.Add(checkIn);
        audit.Record(user.Id, "StopCheckedIn", nameof(StopCheckIn), checkIn.Id, null,
            new { stop.StopId, stop.AttractionName, DistanceMeters = distance, TripRequestId = trip.Id });

        // Trip status: this check-in plus the saved ones; the last stop completes the trip.
        var before = trip.Status;
        var checkedIn = await resources.CountCheckInsOfTripAsync(trip.Id, ct) + 1;
        var totalStops = await resources.CountStopsOfTripAsync(trip.Id, ct);
        trip.Status = checkedIn >= totalStops ? TripRequestStatus.Completed : TripRequestStatus.InProgress;
        if (trip.Status != before)
            audit.Record(user.Id, "TripRequestStatusChanged", nameof(TripRequest), trip.Id,
                new { Status = before.ToString() }, new { Status = trip.Status.ToString() });

        await unitOfWork.SaveChangesAsync(ct);
        return new CheckInResultDto(stop.StopId, distance, checkIn.CheckedInAt, trip.Status.ToString());
    }

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
            .ToDictionaryAsync(v => v.Id, v => v.RegistrationNo, ct);

        var result = new List<GuideTripDto>();
        foreach (var trip in heldTrips)
        {
            var itinerary = await trips.GetItineraryAsync(trip.Id, ct);
            var days = await DaysAsync(trip, itinerary, ct);
            var vehicleId = vehicleHolds.FirstOrDefault(h => h.TripRequestId == trip.Id)?.ResourceId;
            result.Add(new GuideTripDto(trip.Id, trip.Objective, trip.StartDate, trip.EndDate, trip.Pax,
                trip.Status.ToString(), vehicleId is { } id ? vehicles.GetValueOrDefault(id) : null, days));
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
