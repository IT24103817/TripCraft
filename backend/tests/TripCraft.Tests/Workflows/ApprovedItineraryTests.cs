using FluentAssertions;
using TripCraft.Application.Trips;
using TripCraft.Application.Workflows;
using TripCraft.Application.Workflows.Dtos;
using S = TripCraft.Tests.Workflows.Fakes.FakeResourcesState;

namespace TripCraft.Tests.Workflows;

/// <summary>The approved proposal becomes the saved itinerary (PLAN.md section 6 step 11).</summary>
public class ApprovedItineraryTests
{
    private static readonly DateOnly Start = new(2026, 10, 10);
    private static readonly DemoAttractions A = DemoAttractions.Random;

    private static Itinerary Build()
    {
        var p = TestProposals.Golden(Start, A);
        return ApprovedItinerary.Build(Guid.NewGuid(), new StoredProposal(p.Days, p.Resources, p.Quotation, [], 0, null));
    }

    [Fact]
    public void Every_proposal_day_becomes_a_day_with_its_stops_in_order()
    {
        var itinerary = Build();

        itinerary.GeneratedBy.Should().Be(ItinerarySource.Agent);
        itinerary.Days.Select(d => (d.DayNumber, d.City)).Should().Equal(
            (1, "Kandy"), (2, "Kandy"), (3, "Ella"), (4, "Ella"), (5, "Ella"));
        itinerary.Days[0].Stops.Should().ContainSingle(s => s.AttractionId == A.Temple && s.Sequence == 1);
    }

    [Fact]
    public void Each_day_gets_the_hotel_of_its_night_and_the_last_day_has_none()
    {
        var days = Build().Days;

        days[0].HotelId.Should().Be(S.KandyHotel);
        days[2].HotelId.Should().Be(S.EllaHotel);
        days[4].HotelId.Should().BeNull();
    }

    [Fact]
    public void Transport_is_kept_in_the_notes_and_bad_attraction_ids_are_skipped()
    {
        var p = TestProposals.Golden(Start, A);
        var days = p.Days!.ToList();
        days[0] = days[0] with { Stops = [new ProposalStop("not-a-guid", "Bad", 0), TestProposals.Stop(A.Temple, 0)] };

        var itinerary = ApprovedItinerary.Build(Guid.NewGuid(), new StoredProposal(days, p.Resources, p.Quotation, [], 0, null));

        itinerary.Days[0].Stops.Should().ContainSingle().Which.Sequence.Should().Be(1);
        itinerary.Days[2].Notes.Should().Be("By train");
    }
}
