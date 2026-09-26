using FluentAssertions;
using TripCraft.Application.Resources.Services;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Tests.Resources;

/// <summary>What the Resource &amp; Action agent sees through the internal availability tools.</summary>
public class ResourceCatalogTests : IDisposable
{
    private static readonly DateOnly Start = new(2026, 10, 10);
    private readonly ResourceTestDb _t = new();

    private ResourceCatalog Catalog() => new(_t.Repository);

    [Fact]
    public async Task Guides_must_speak_the_language_take_the_group_and_be_free_cheapest_first()
    {
        var english = await Catalog().FindAvailableGuidesAsync(Start, Start.AddDays(4), "EN", 4, CancellationToken.None);
        english.Select(g => g.Name).Should().Equal("Nimal", "Kumari");

        (await Catalog().FindAvailableGuidesAsync(Start, Start.AddDays(4), "en", 6, CancellationToken.None))
            .Should().ContainSingle(g => g.Name == "Nimal", "Kumari takes only 4");

        _t.Hold(ResourceType.Guide, ResourceTestDb.EnglishGuide, Start.AddDays(2), Start.AddDays(2));
        (await Catalog().FindAvailableGuidesAsync(Start, Start.AddDays(4), "en", 4, CancellationToken.None))
            .Should().ContainSingle(g => g.Name == "Kumari");
    }

    [Fact]
    public async Task Vehicles_need_enough_seats()
    {
        var vehicles = await Catalog().FindAvailableVehiclesAsync(Start, Start.AddDays(1), 4, CancellationToken.None);

        vehicles.Should().ContainSingle(v => v.RegistrationNo == "VAN-1");
    }

    [Fact]
    public async Task Rooms_report_the_free_count_for_the_night()
    {
        _t.Hold(ResourceType.Room, ResourceTestDb.KandyDouble, Start, Start, quantity: 2);

        (await Catalog().FindAvailableRoomsAsync("kandy", Start, 1, CancellationToken.None))
            .Should().ContainSingle().Which.AvailableRooms.Should().Be(1);
        (await Catalog().FindAvailableRoomsAsync("Kandy", Start, 2, CancellationToken.None)).Should().BeEmpty();
        (await Catalog().FindAvailableRoomsAsync("Kandy", Start.AddDays(1), 2, CancellationToken.None))
            .Should().ContainSingle().Which.AvailableRooms.Should().Be(3);
    }

    [Fact]
    public async Task Rate_card_has_the_margin_and_every_rate()
    {
        var card = await Catalog().GetRateCardAsync(CancellationToken.None);

        card.MarginPct.Should().Be(12m);
        card.GuideDayRates[ResourceTestDb.EnglishGuide].Should().Be(6000m);
        card.VehicleKmRates[ResourceTestDb.Van].Should().Be(120m);
        card.RoomNightRates[ResourceTestDb.KandyDouble].Should().Be(12000m);
    }

    public void Dispose() => _t.Dispose();
}
