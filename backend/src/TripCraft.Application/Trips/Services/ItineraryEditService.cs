using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Common;
using TripCraft.Application.Common.Auditing;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Trips.Dtos;

namespace TripCraft.Application.Trips.Services;

public interface IItineraryEditService
{
    Task<ItineraryDto> EditDayAsync(Guid actorId, Guid tripId, int dayNumber, EditItineraryDayRequest request,
        CancellationToken ct);
}

/// <summary>
/// The itinerary editor (PLAN.md section 3, React "itinerary editor"): an Operations Manager changes one day's
/// stops and notes of a Confirmed trip, before the tour starts. The day's city stays; every stop must be an
/// active attraction in that city. The itinerary version goes up, it is marked Manual, and the change is audited.
/// </summary>
public class ItineraryEditService(
    ITripRequestRepository trips,
    IAttractionRepository attractions,
    IAuditLogger audit,
    IUnitOfWork unitOfWork) : IItineraryEditService
{
    public async Task<ItineraryDto> EditDayAsync(Guid actorId, Guid tripId, int dayNumber,
        EditItineraryDayRequest request, CancellationToken ct)
    {
        var trip = await trips.GetByIdAsync(tripId, ct) ?? throw new NotFoundException("Trip request not found.");
        if (trip.Status != TripRequestStatus.Confirmed)
            throw new ConflictException($"The itinerary can only be edited while the trip is Confirmed; it is {trip.Status}.");

        var itinerary = await trips.GetItineraryForUpdateAsync(tripId, ct)
                        ?? throw new ConflictException("This trip request has no saved itinerary to edit.");
        var day = itinerary.Days.FirstOrDefault(d => d.DayNumber == dayNumber)
                  ?? throw new NotFoundException($"Day {dayNumber} is not in this itinerary.");

        var ids = request.AttractionIds.ToList();
        var found = await attractions.QueryActive().Where(a => ids.Contains(a.Id))
            .Select(a => new { a.Id, a.Name, a.City }).ToListAsync(ct);
        var errors = ids.Where(id => found.All(a => a.Id != id))
            .Select(id => new ValidationFailure("attractionIds", $"Attraction {id} does not exist."))
            .Concat(found.Where(a => !string.Equals(a.City, day.City, StringComparison.OrdinalIgnoreCase))
                .Select(a => new ValidationFailure("attractionIds", $"{a.Name} is in {a.City}, not {day.City}.")))
            .ToList();
        if (errors.Count > 0)
            throw new ValidationException(errors);

        var before = new { Day = dayNumber, Stops = day.Stops.OrderBy(s => s.Sequence).Select(s => s.AttractionId), day.Notes };
        day.Stops.Clear(); // the removed stops are deleted (required relationship)
        for (var i = 0; i < ids.Count; i++)
            trips.AddItineraryStop(new ItineraryStop { ItineraryDayId = day.Id, AttractionId = ids[i], Sequence = i + 1 });
        day.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        itinerary.Version++;
        itinerary.GeneratedBy = ItinerarySource.Manual;

        audit.Record(actorId, "ItineraryDayEdited", nameof(TripRequest), trip.Id, before,
            new { Day = dayNumber, Stops = ids.Select(id => found.First(a => a.Id == id).Name), day.Notes, itinerary.Version });
        await unitOfWork.SaveChangesAsync(ct);

        return ItineraryDto.FromEntity((await trips.GetItineraryAsync(tripId, ct))!);
    }
}
