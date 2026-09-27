using TripCraft.Application.Trips;

namespace TripCraft.Application.Workflows;

/// <summary>
/// PLAN.md section 6 step 11: when a manager approves, the approved proposal becomes the trip's saved itinerary
/// (itineraries → itinerary_days → itinerary_stops). Pure: the approval transaction adds the result.
/// </summary>
public static class ApprovedItinerary
{
    public static Itinerary Build(Guid tripRequestId, StoredProposal proposal)
    {
        var rooms = proposal.Resources?.Rooms ?? [];
        var itinerary = new Itinerary { TripRequestId = tripRequestId, Version = 1, GeneratedBy = ItinerarySource.Agent };

        foreach (var day in (proposal.Days ?? []).OrderBy(d => d.Day))
        {
            // The hotel of the night that starts on this date (the last day has no night).
            var hotelId = rooms.Where(r => r.Night == day.Date)
                .Select(r => ProposalValidator.ParseId(r.HotelId))
                .FirstOrDefault(id => id is not null);

            var itineraryDay = new ItineraryDay
            {
                ItineraryId = itinerary.Id,
                DayNumber = day.Day,
                City = day.City,
                HotelId = hotelId,
                Notes = day.Weather is null ? $"By {day.Transport}" : $"By {day.Transport}; weather: {day.Weather}"
            };

            var sequence = 1;
            foreach (var stop in day.Stops ?? [])
            {
                if (ProposalValidator.ParseId(stop.AttractionId) is not { } attractionId)
                    continue; // the validator already rejects unknown ids before a proposal can be approved
                itineraryDay.Stops.Add(new ItineraryStop
                {
                    ItineraryDayId = itineraryDay.Id, AttractionId = attractionId, Sequence = sequence++
                });
            }

            itinerary.Days.Add(itineraryDay);
        }

        return itinerary;
    }
}
