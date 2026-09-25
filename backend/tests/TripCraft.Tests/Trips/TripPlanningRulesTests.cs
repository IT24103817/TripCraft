using FluentAssertions;
using TripCraft.Application.Trips;
using TripCraft.Application.Trips.Planning;

namespace TripCraft.Tests.Trips;

/// <summary>Unit tests for the Component A business rules. No database or HTTP involved.</summary>
public class TripPlanningRulesTests
{
    private static readonly DateOnly Today = new(2026, 10, 1);

    private static TripRequest ValidTrip() => new()
    {
        Objective = "5 days in Kandy and Ella",
        StartDate = new DateOnly(2026, 10, 10),
        EndDate = new DateOnly(2026, 10, 14),
        Pax = 4,
        BudgetUsd = 1500
    };

    private static Tourist ValidTourist() => new() { PassportNumberMasked = "****4521" };

    [Fact]
    public void Five_days_over_two_cities_gives_the_extra_day_to_the_first_city()
    {
        var skeleton = TripPlanningRules.BuildSkeleton(
            new DateOnly(2026, 10, 10), new DateOnly(2026, 10, 14), ["Kandy", "Ella"], pace: null);

        skeleton.Select(d => d.City).Should().Equal("Kandy", "Kandy", "Kandy", "Ella", "Ella");
        skeleton.Select(d => d.DayNumber).Should().Equal(1, 2, 3, 4, 5);
        skeleton[4].Date.Should().Be(new DateOnly(2026, 10, 14));
        skeleton.Should().OnlyContain(d => d.MaxStops == 3);
    }

    [Fact]
    public void Relaxed_pace_allows_two_stops_per_day()
    {
        var skeleton = TripPlanningRules.BuildSkeleton(Today, Today.AddDays(1), ["Galle"], pace: "Relaxed");

        skeleton.Should().OnlyContain(d => d.MaxStops == 2);
    }

    [Fact]
    public void More_cities_than_days_is_rejected()
    {
        var act = () => TripPlanningRules.BuildSkeleton(Today, Today, ["Kandy", "Ella"], pace: null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Cities_are_returned_in_the_order_they_are_mentioned()
    {
        var cities = TripPlanningRules.ExtractCities(
            "Start in ella then finish in Kandy", ["Colombo", "Kandy", "Ella", "Galle"]);

        cities.Should().Equal("Ella", "Kandy");
    }

    [Fact]
    public void City_names_only_match_whole_words()
    {
        var cities = TripPlanningRules.ExtractCities("Bring an umbrella to Galle", ["Ella", "Galle"]);

        cities.Should().Equal("Galle");
    }

    [Fact]
    public void Valid_trip_has_no_planning_errors()
    {
        TripPlanningRules.ValidateForPlanning(ValidTrip(), ValidTourist(), Today).Should().BeEmpty();
    }

    [Fact]
    public void Past_start_date_and_missing_passport_are_both_reported()
    {
        var trip = ValidTrip();
        trip.StartDate = Today.AddDays(-1);
        var tourist = new Tourist { PassportNumberMasked = "" };

        var errors = TripPlanningRules.ValidateForPlanning(trip, tourist, Today);

        errors.Should().Contain("Start date is in the past.")
              .And.Contain("Tourist has no valid passport number on file.");
    }

    [Fact]
    public void Trip_longer_than_thirty_days_is_rejected()
    {
        var trip = ValidTrip();
        trip.EndDate = trip.StartDate.AddDays(30);

        TripPlanningRules.ValidateForPlanning(trip, ValidTourist(), Today)
            .Should().ContainSingle(e => e.Contains("30 days"));
    }

    [Theory]
    [InlineData("N1234567", "****4567")]
    [InlineData("ab 12 34 xy", "****34XY")]
    public void Passport_is_masked_to_last_four_characters(string input, string expected)
    {
        TripPlanningRules.MaskPassport(input).Should().Be(expected);
    }

    [Theory]
    [InlineData("""{"pace":"relaxed"}""", "relaxed")]
    [InlineData("""{"language":"en"}""", null)]
    [InlineData("{}", null)]
    public void Pace_is_read_from_preferences(string json, string? expected)
    {
        TripPlanningRules.ReadPace(json).Should().Be(expected);
    }
}
