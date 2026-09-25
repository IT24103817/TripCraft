using FluentAssertions;
using TripCraft.Application.Trips;
using TripCraft.Application.Workflows;
using TripCraft.Application.Workflows.Dtos;
using TripCraft.Tests.Workflows.Fakes;
using static TripCraft.Tests.Workflows.TestProposals;
using S = TripCraft.Tests.Workflows.Fakes.FakeResourcesState;

namespace TripCraft.Tests.Workflows;

/// <summary>Golden cases for the deterministic validator (PLAN.md section 5). Pure: no database, no HTTP.</summary>
public class ProposalValidatorTests
{
    private static readonly DateOnly Start = new(2026, 10, 10);
    private static readonly DemoAttractions A = DemoAttractions.Random;
    private readonly ProposalValidator _validator = new();
    private readonly S _resources = new();

    private static TripRequest Trip(decimal budgetUsd = 1500, string language = "en") => new()
    {
        StartDate = Start, EndDate = Start.AddDays(4), Pax = 4, BudgetUsd = budgetUsd,
        Preferences = $$"""{"language":"{{language}}","transport":"train"}"""
    };

    private ProposalFacts Facts(Guid? vehicleId = null, bool guideOverlaps = false, bool vehicleOverlaps = false,
        IReadOnlyDictionary<Guid, decimal>? fees = null) => new(
        fees ?? A.Fees,
        _resources.Guides.Single(),
        _resources.Vehicles.Single(v => v.Id == (vehicleId ?? S.VanSixSeats)),
        _resources.Rooms.ToDictionary(r => r.Room.RoomTypeId, r => r.Room),
        guideOverlaps, vehicleOverlaps, _resources.RateCard);

    private static IEnumerable<string> Codes(ProposalValidationResult r) => r.Violations.Select(v => v.Code);

    [Fact]
    public void Golden_proposal_is_valid()
    {
        var result = _validator.Validate(Golden(Start, A), Trip(), Facts());

        result.IsValid.Should().BeTrue();
        result.Violations.Should().BeEmpty();
    }

    [Fact]
    public void Missing_attraction_id_is_a_hard_violation()
    {
        var fees = A.Fees.Where(f => f.Key != A.AdamsPeak).ToDictionary(f => f.Key, f => f.Value);

        var result = _validator.Validate(Golden(Start, A), Trip(), Facts(fees: fees));

        result.IsValid.Should().BeFalse();
        result.Violations.Should().ContainSingle(v => v.Code == "UNKNOWN_ATTRACTION" && v.Severity == ViolationSeverity.Hard);
    }

    [Fact]
    public void Overlapping_guide_hold_is_a_hard_violation()
    {
        var result = _validator.Validate(Golden(Start, A), Trip(), Facts(guideOverlaps: true));

        Codes(result).Should().Equal("GUIDE_HOLD_OVERLAP");
        result.HasHard.Should().BeTrue();
    }

    [Fact]
    public void Rooms_below_pax_on_a_night_is_a_hard_violation()
    {
        var proposal = Golden(Start, A, totalLkr: 173420m); // one room-night less: 162800 - 12000 = 150800 x 1.15
        proposal.Resources!.Rooms!.RemoveAt(0);

        var result = _validator.Validate(proposal, Trip(), Facts());

        Codes(result).Should().Equal("ROOMS_BELOW_PAX");
        result.Violations[0].Message.Should().Contain("sleeps 2 but pax is 4");
    }

    [Fact]
    public void Vehicle_with_fewer_seats_than_pax_is_a_hard_violation()
    {
        var proposal = Golden(Start, A, totalLkr: 184000m) with // car: 140 km x 100 = 14000 -> 160000 x 1.15
        {
            Resources = Golden(Start, A).Resources! with { VehicleId = S.CarThreeSeats.ToString() }
        };

        var result = _validator.Validate(proposal, Trip(), Facts(vehicleId: S.CarThreeSeats));

        Codes(result).Should().Equal("VEHICLE_SEATS");
    }

    [Fact]
    public void Guide_not_speaking_the_requested_language_is_a_hard_violation()
    {
        var result = _validator.Validate(Golden(Start, A), Trip(language: "de"), Facts());

        Codes(result).Should().Equal("GUIDE_LANGUAGE");
    }

    [Fact]
    public void Four_stops_in_a_day_is_a_hard_violation()
    {
        var proposal = Golden(Start, A);
        proposal.Days![3] = Day(4, Start.AddDays(3), "Ella", 0,
            Stop(A.AdamsPeak, 0), Stop(A.NineArches, 0), Stop(A.AdamsPeak, 0), Stop(A.NineArches, 0)); // free stops: total unchanged

        var result = _validator.Validate(proposal, Trip(), Facts());

        Codes(result).Should().Equal("DAY_STOPS");
        result.Violations[0].Message.Should().Contain("Day 4 has 4 stops");
    }

    [Fact]
    public void Over_budget_is_the_only_soft_violation()
    {
        var result = _validator.Validate(Golden(Start, A), Trip(budgetUsd: 400), Facts());

        result.IsValid.Should().BeFalse();
        result.HasHard.Should().BeFalse();
        result.Violations.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new { Code = "OVER_BUDGET", Severity = ViolationSeverity.Soft });
        result.Violations[0].Message.Should().Contain("624.07");
    }

    [Fact]
    public void Quotation_total_more_than_1_lkr_off_the_server_total_is_a_hard_violation()
    {
        _validator.Validate(Golden(Start, A, totalLkr: GoldenTotalLkr + 1m), Trip(), Facts()).IsValid.Should().BeTrue();

        var result = _validator.Validate(Golden(Start, A, totalLkr: GoldenTotalLkr - 5000m), Trip(), Facts());

        Codes(result).Should().Equal("QUOTATION_MISMATCH");
    }

    [Fact]
    public void Incomplete_json_structure_is_a_hard_violation_and_stops_other_checks()
    {
        var proposal = Golden(Start, A) with { Days = null, Quotation = null };

        var result = _validator.Validate(proposal, Trip(), Facts());

        Codes(result).Should().OnlyContain(c => c == "SCHEMA_INCOMPLETE").And.HaveCount(2);
    }

    [Fact]
    public void Unknown_guide_vehicle_and_room_ids_are_hard_violations()
    {
        var proposal = Golden(Start, A) with
        {
            Resources = new ProposalResources("not-a-guid", Guid.NewGuid().ToString(),
                [Room(S.KandyHotel, Guid.NewGuid(), Start)], [])
        };

        var result = _validator.Validate(proposal, Trip(), Facts() with { Guide = null, Vehicle = null });

        Codes(result).Should().Contain(["UNKNOWN_GUIDE", "UNKNOWN_VEHICLE", "UNKNOWN_ROOM_TYPE"]);
    }
}
