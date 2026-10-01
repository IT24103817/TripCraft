using FluentAssertions;
using TripCraft.Application.Trips.Templates;

namespace TripCraft.Tests.Trips;

public class TemplatePricingTests
{
    private static readonly Dictionary<string, IReadOnlyList<(int Capacity, decimal RatePerNightLkr)>> Rooms = new()
    {
        ["Kandy"] = [(2, 12000m), (4, 20000m)],
        ["Ella"] = [(2, 12000m)]
    };

    [Theory]
    [InlineData("Kandy", 2, 12000)]   // one double
    [InlineData("Kandy", 4, 20000)]   // a family room beats two doubles (24,000)
    [InlineData("Kandy", 3, 20000)]   // two doubles 24,000 vs one family room 20,000
    [InlineData("Ella", 4, 24000)]    // only doubles: two of them
    [InlineData("Galle", 2, 0)]       // no hotel known: nothing priced
    public void The_cheapest_way_to_sleep_the_party_is_used(string city, int pax, decimal expected)
    {
        TemplatePricing.CheapestNight(city, pax, Rooms).Should().Be(expected);
    }

    [Fact]
    public void A_package_is_priced_like_a_quotation_guide_vehicle_nights_entries_and_margin()
    {
        List<TemplateDay> days = [new(1, "Kandy", ["Temple"]), new(2, "Ella", ["Bridge"]), new(3, "Ella", [])];
        var inputs = new TemplatePriceInputs(6000, 100, Rooms,
            new Dictionary<string, decimal> { ["Temple"] = 2000, ["Bridge"] = 0 }, [140], 15, 300);

        var price = TemplatePricing.Calculate(days, 2, inputs);

        // guide 3 x 6000 + vehicle 140 x 100 + nights Kandy 12000 + Ella 12000 + temple 2 x 2000 = 60,000
        price.SubtotalLkr.Should().Be(60000);
        price.TotalLkr.Should().Be(69000);   // + 15 % margin
        price.TotalUsd.Should().Be(230);     // / 300 LKR per USD
    }
}
