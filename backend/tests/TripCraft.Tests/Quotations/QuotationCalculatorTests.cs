using FluentAssertions;
using TripCraft.Application.Quotations;
using TripCraft.Application.Quotations.Reports;

namespace TripCraft.Tests.Quotations;

/// <summary>PLAN.md section 11: quotation calculation (Component C business rule, pure).</summary>
public class QuotationCalculatorTests
{
    [Fact]
    public void Golden_demo_trip_totals_match_the_agent_formula()
    {
        // 5 days, guide 6000/day; van 140 km x 120; 8 room-nights x 12000; entry (2000 + 3000) x 4 pax; 15 %; 300 LKR/USD.
        var items = new[]
        {
            new PriceItem("guide", "Guide", 5, 6000), new PriceItem("vehicle", "Van", 140, 120),
            new PriceItem("room", "Rooms", 8, 12000), new PriceItem("entry", "Temple", 4, 2000),
            new PriceItem("entry", "Gardens", 4, 3000)
        };

        var q = QuotationCalculator.Calculate(items, 15m, 300m);

        q.SubtotalLkr.Should().Be(162_800m);
        q.MarginLkr.Should().Be(24_420m);
        q.TotalLkr.Should().Be(187_220m);
        q.TotalUsd.Should().Be(624.07m);
        q.Lines.Should().HaveCount(5);
    }

    [Fact]
    public void Free_entries_and_zero_km_are_left_out_and_every_amount_rounds_half_away_from_zero()
    {
        var items = new[]
        {
            new PriceItem("entry", "Free bridge", 4, 0), new PriceItem("vehicle", "Van", 0, 120),
            new PriceItem("vehicle", "Van", 12.345m, 100)
        };

        var q = QuotationCalculator.Calculate(items, 10m, 3m);

        q.Lines.Should().ContainSingle().Which.AmountLkr.Should().Be(1234.50m);
        q.MarginLkr.Should().Be(123.45m);
        q.TotalLkr.Should().Be(1357.95m);
        q.TotalUsd.Should().Be(452.65m);
    }

    [Theory]
    [InlineData(0, 15)]
    [InlineData(-300, 15)]
    [InlineData(300, 101)]
    public void An_invalid_rate_or_margin_is_refused(decimal fx, decimal margin)
    {
        var act = () => QuotationCalculator.Calculate([new PriceItem("guide", "Guide", 1, 1)], margin, fx);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(1, 31, 5, 10, 6)]   // inside the range
    [InlineData(1, 31, -3, 2, 2)]   // starts before the range
    [InlineData(1, 31, 29, 40, 3)]  // ends after the range
    [InlineData(1, 31, 32, 35, 0)]  // outside
    public void ReportMath_clips_holds_to_the_range(int from, int to, int holdFrom, int holdTo, int expected)
    {
        var day1 = new DateOnly(2026, 10, 1);
        ReportMath.ClippedDays(day1.AddDays(holdFrom - 1), day1.AddDays(holdTo - 1), day1.AddDays(from - 1), day1.AddDays(to - 1))
            .Should().Be(expected);
    }

    [Fact]
    public void ReportMath_percent_has_one_decimal_and_handles_an_empty_range()
    {
        ReportMath.Percent(10, 31).Should().Be(32.3m);
        ReportMath.Percent(1, 0).Should().Be(0m);
    }
}
