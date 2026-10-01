using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TripCraft.Application.Resources.Dtos;
using TripCraft.Application.Workflows.Ports;
using TripCraft.Infrastructure.Resources;
using TripCraft.Tests.Common;

namespace TripCraft.Tests.Resources;

/// <summary>The resource-by-day grid and editing manual blocks (v1.1).</summary>
public class AvailabilityGridTests
{
    private static string Range(DateOnly from, int days) => $"from={from:yyyy-MM-dd}&to={from.AddDays(days - 1):yyyy-MM-dd}";

    [Fact]
    public async Task Cells_show_confirmed_trips_with_the_tourist_blocks_and_free_days()
    {
        await using var factory = new TestWebApplicationFactory();
        var trip = await ConfirmedTrip.CreateAsync(factory, startInDays: 5);
        var start = Application.Common.Settings.TripSettings.Default.Today().AddDays(5);
        var manager = await factory.CreateClientAsAsync("manager1@tripcraft.test");
        var block = await manager.PostAsJsonAsync("/api/resource-holds", new CreateHoldRequest(ResourceType.Vehicle,
            Guid.Parse("00000000-0000-0000-0000-00000000b003"), start, start, 1, "Maintenance"));
        block.EnsureSuccessStatusCode();

        var grid = await manager.GetFromJsonAsync<AvailabilityGridDto>($"/api/availability/grid?{Range(start.AddDays(-1), 7)}", TestJson.Options);

        grid!.Days.Should().HaveCount(7);
        var nimal = grid.Rows.Single(r => r.ResourceId == ResourcesSeeder.NimalGuide);
        nimal.Cells[0].State.Should().Be("Free");
        nimal.Cells[1].Should().Match<AvailabilityCellDto>(c => c.State == "Confirmed" && c.TripRequestId == trip.TripId
                                                                 && c.TouristName == "Demo Tourist 1" && c.FreeQuantity == 0);
        var coach = grid.Rows.Single(r => r.Name == "NC-4455");
        coach.Cells[1].Should().Match<AvailabilityCellDto>(c => c.State == "Blocked" && c.Note == "Maintenance");
        grid.Rows.Where(r => r.ResourceType == ResourceType.Room).Should().OnlyContain(r => r.Capacity > 1);
    }

    [Fact]
    public async Task Filters_narrow_the_rows_and_the_range_is_limited()
    {
        await using var factory = new TestWebApplicationFactory();
        var manager = await factory.CreateClientAsAsync("manager1@tripcraft.test");
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var vans = await manager.GetFromJsonAsync<AvailabilityGridDto>($"/api/availability/grid?{Range(today, 7)}&type=Vehicle&seats=6", TestJson.Options);
        var ella = await manager.GetFromJsonAsync<AvailabilityGridDto>($"/api/availability/grid?{Range(today, 7)}&type=Room&city=Ella", TestJson.Options);
        var german = await manager.GetFromJsonAsync<AvailabilityGridDto>($"/api/availability/grid?{Range(today, 7)}&type=Guide&language=de", TestJson.Options);

        vans!.Rows.Should().OnlyContain(r => r.ResourceType == ResourceType.Vehicle).And.NotContain(r => r.Name == "CAR-9876");
        ella!.Rows.Should().OnlyContain(r => r.Detail.StartsWith("Ella"));
        german!.Rows.Should().ContainSingle(r => r.Name == "Kumari Silva");
        (await manager.GetAsync($"/api/availability/grid?{Range(today, 63)}")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await (await factory.CreateClientAsAsync("tourist1@tripcraft.test")).GetAsync($"/api/availability/grid?{Range(today, 7)}"))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task A_manual_block_can_be_read_and_moved_but_a_trips_hold_cannot_be_edited()
    {
        await using var factory = new TestWebApplicationFactory();
        var trip = await ConfirmedTrip.CreateAsync(factory, startInDays: 5);
        var manager = await factory.CreateClientAsAsync("manager1@tripcraft.test");
        var day = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(20);
        var created = (await (await manager.PostAsJsonAsync("/api/resource-holds", new CreateHoldRequest(ResourceType.Guide,
            ResourcesSeeder.NimalGuide, day, day, 1, "Annual leave"))).Content.ReadFromJsonAsync<HoldDto>(TestJson.Options))!;

        var read = await manager.GetFromJsonAsync<HoldDto>($"/api/resource-holds/{created.Id}", TestJson.Options);
        var moved = await manager.PutAsJsonAsync($"/api/resource-holds/{created.Id}", new UpdateBlockRequest(day.AddDays(1), day.AddDays(3), 1, "Annual leave (extended)"));
        var tripHold = (await manager.GetFromJsonAsync<AvailabilityGridDto>(
            $"/api/availability/grid?{Range(Application.Common.Settings.TripSettings.Default.Today().AddDays(5), 1)}&type=Guide", TestJson.Options))!
            .Rows.Single(r => r.ResourceId == ResourcesSeeder.NimalGuide).Cells[0].HoldId!.Value;
        var editTrip = await manager.PutAsJsonAsync($"/api/resource-holds/{tripHold}", new UpdateBlockRequest(day, day, 1, "x"));

        read!.Note.Should().Be("Annual leave");
        moved.StatusCode.Should().Be(HttpStatusCode.OK);
        var after = (await moved.Content.ReadFromJsonAsync<HoldDto>(TestJson.Options))!;
        after.FromDate.Should().Be(day.AddDays(1));
        after.ToDate.Should().Be(day.AddDays(3));
        after.Note.Should().Be("Annual leave (extended)");
        (await manager.GetFromJsonAsync<HoldDto>($"/api/resource-holds/{created.Id}", TestJson.Options))!.Status.Should().Be("Released");
        editTrip.StatusCode.Should().Be(HttpStatusCode.Conflict);
        trip.TripId.Should().NotBeEmpty();
    }

    [Fact]
    public void A_partly_held_room_type_shows_the_free_count()
    {
        var day = new DateOnly(2026, 11, 1);
        var hold = new Application.Resources.ResourceHold { ResourceType = ResourceType.Room, Quantity = 2, TripRequestId = Guid.NewGuid(),
            FromDate = day, ToDate = day };

        var cell = Application.Resources.Services.AvailabilityGridService.Cell(day, 5, [hold],
            new Dictionary<Guid, (string, Application.Trips.TripRequestStatus)> { [hold.TripRequestId!.Value] = ("Ann", Application.Trips.TripRequestStatus.ClientAccepted) });

        cell.State.Should().Be("Held");
        cell.HeldQuantity.Should().Be(2);
        cell.FreeQuantity.Should().Be(3);
        cell.TouristName.Should().Be("Ann");
    }
}
