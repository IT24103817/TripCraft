using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Common;
using TripCraft.Application.Common.Auditing;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Common.Security;
using TripCraft.Application.Resources.Dtos;
using TripCraft.Application.Trips;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Application.Resources.Services;

public interface ITripAssignmentService
{
    Task<TripAssignmentDto> GetAsync(CurrentUser user, Guid tripRequestId, CancellationToken ct);
    Task<GuideRatingDto> GetRatingAsync(CurrentUser user, Guid tripRequestId, CancellationToken ct);
    Task<GuideRatingDto> RateGuideAsync(CurrentUser user, Guid tripRequestId, RateGuideRequest request, CancellationToken ct);
}

/// <summary>
/// The tourist's view of the booked resources (from the trip's Held holds) and the guide rating after the tour (v1.1).
/// A tourist only sees their own trips (403 otherwise).
/// </summary>
public class TripAssignmentService(
    IResourceRepository resources,
    ITripRequestRepository trips,
    IAuditLogger audit,
    IUnitOfWork unitOfWork) : ITripAssignmentService
{
    public async Task<TripAssignmentDto> GetAsync(CurrentUser user, Guid tripRequestId, CancellationToken ct)
    {
        var trip = await LoadAsync(user, tripRequestId, ct);
        var held = await resources.Holds()
            .Where(h => h.TripRequestId == trip.Id && h.Status == HoldStatus.Held && h.ResourceType != ResourceType.Room)
            .ToListAsync(ct);
        var guideId = held.FirstOrDefault(h => h.ResourceType == ResourceType.Guide)?.ResourceId;
        var vehicleId = held.FirstOrDefault(h => h.ResourceType == ResourceType.Vehicle)?.ResourceId;
        var guide = guideId is { } g ? await resources.Guides().FirstOrDefaultAsync(x => x.Id == g, ct) : null;
        var vehicle = vehicleId is { } v ? await resources.Vehicles().FirstOrDefaultAsync(x => x.Id == v, ct) : null;
        return new TripAssignmentDto(trip.Id, guide?.Id, guide?.Name, guide?.Phone, vehicle?.RegistrationNo,
            vehicle?.Type, vehicle?.Seats);
    }

    public async Task<GuideRatingDto> GetRatingAsync(CurrentUser user, Guid tripRequestId, CancellationToken ct)
    {
        var trip = await LoadAsync(user, tripRequestId, ct);
        var rating = await resources.GuideRatings().FirstOrDefaultAsync(r => r.TripRequestId == trip.Id, ct)
                     ?? throw new NotFoundException("This trip has not been rated yet.");
        return await ToDtoAsync(rating, ct);
    }

    /// <summary>Owner tourist, Completed trip, once. The guide is the one held for the trip.</summary>
    public async Task<GuideRatingDto> RateGuideAsync(CurrentUser user, Guid tripRequestId, RateGuideRequest request,
        CancellationToken ct)
    {
        var trip = await LoadAsync(user, tripRequestId, ct);
        if (!user.IsTourist)
            throw new ForbiddenException("Only the tourist rates the guide.");
        if (trip.Status != TripRequestStatus.Completed)
            throw new ConflictException("You can rate the guide once the trip is completed.");
        if (await resources.GuideRatings().AnyAsync(r => r.TripRequestId == trip.Id, ct))
            throw new ConflictException("You have already rated the guide of this trip.");
        var guideId = await resources.Holds()
            .Where(h => h.TripRequestId == trip.Id && h.ResourceType == ResourceType.Guide && h.Status == HoldStatus.Held)
            .Select(h => (Guid?)h.ResourceId).FirstOrDefaultAsync(ct)
            ?? throw new ConflictException("This trip has no guide to rate.");

        var rating = new GuideRating
        {
            TripRequestId = trip.Id, GuideId = guideId, RatedByUserId = user.Id, Stars = request.Stars,
            Comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim()
        };
        resources.Add(rating);
        audit.Record(user.Id, "GuideRated", nameof(GuideRating), rating.Id, null, new { rating.GuideId, rating.Stars });
        await unitOfWork.SaveChangesAsync(ct);
        return await ToDtoAsync(rating, ct);
    }

    private async Task<TripRequest> LoadAsync(CurrentUser user, Guid tripRequestId, CancellationToken ct)
    {
        var trip = await trips.GetByIdAsync(tripRequestId, ct) ?? throw new NotFoundException("Trip request not found.");
        TripAccess.EnsureCanAccess(user, trip.Tourist?.UserId);
        return trip;
    }

    private async Task<GuideRatingDto> ToDtoAsync(GuideRating r, CancellationToken ct)
    {
        var name = await resources.Guides().Where(g => g.Id == r.GuideId).Select(g => g.Name).FirstOrDefaultAsync(ct);
        return new GuideRatingDto(r.TripRequestId, r.GuideId, name ?? "Guide", r.Stars, r.Comment, r.CreatedAt);
    }
}
