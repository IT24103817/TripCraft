using FluentAssertions;
using TripCraft.Application.Resources;

namespace TripCraft.Tests.Resources;

/// <summary>PLAN.md section 11: availability overlap logic (pure, no database).</summary>
public class AvailabilityRulesTests
{
    private static readonly DateOnly D10 = new(2026, 10, 10);

    [Theory]
    [InlineData(10, 14, 12, 16, true)]  // partial overlap
    [InlineData(10, 14, 14, 18, true)]  // touching on the last day counts (dates are inclusive)
    [InlineData(10, 14, 15, 18, false)] // the next day is free
    [InlineData(12, 13, 10, 14, true)]  // inside
    [InlineData(10, 10, 10, 10, true)]  // same single day
    public void Overlaps_uses_inclusive_dates(int aFrom, int aTo, int bFrom, int bTo, bool expected)
    {
        AvailabilityRules.Overlaps(D10.AddDays(aFrom - 10), D10.AddDays(aTo - 10), D10.AddDays(bFrom - 10), D10.AddDays(bTo - 10))
            .Should().Be(expected);
        AvailabilityRules.Overlaps(D10.AddDays(bFrom - 10), D10.AddDays(bTo - 10), D10.AddDays(aFrom - 10), D10.AddDays(aTo - 10))
            .Should().Be(expected, "overlap is symmetric");
    }

    [Theory]
    [InlineData(5, new[] { 2, 1 }, 2)]
    [InlineData(5, new int[0], 5)]
    [InlineData(3, new[] { 2, 2 }, 0)] // never negative, even with bad data
    public void FreeRooms_subtracts_held_rooms_and_never_goes_below_zero(int total, int[] held, int expected)
    {
        AvailabilityRules.FreeRooms(total, held).Should().Be(expected);
    }

    [Theory]
    [InlineData(4, 2, 2)]
    [InlineData(5, 2, 3)]
    [InlineData(1, 4, 1)]
    public void RoomsNeeded_rounds_up(int pax, int capacity, int expected)
    {
        AvailabilityRules.RoomsNeeded(pax, capacity).Should().Be(expected);
    }

    [Fact]
    public void DistanceMeters_is_zero_at_the_stop_and_about_1_1_km_for_0_01_degrees_latitude()
    {
        AvailabilityRules.DistanceMeters(6.8768, 81.0608, 6.8768, 81.0608).Should().Be(0);
        AvailabilityRules.DistanceMeters(6.8768, 81.0608, 6.8868, 81.0608).Should().BeInRange(1100, 1125);
    }
}
