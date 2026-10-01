using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Resources;
using TripCraft.Application.Resources.Dtos;
using TripCraft.Application.Workflows.Ports;
using TripCraft.Infrastructure.Resources;
using TripCraft.Tests.Common;

namespace TripCraft.Tests.Resources;

/// <summary>A guide asks to be replaced; the manager picks a same-language guide; the holds are swapped (v1.1).</summary>
public class GuideChangeRequestTests
{
    private static readonly Guid Kumari = Guid.Parse("00000000-0000-0000-0000-00000000a002");

    [Fact]
    public async Task Guide_asks_manager_swaps_to_a_candidate_and_everyone_is_notified()
    {
        await using var factory = new RealComponentsFactory();
        var trip = await ConfirmedTrip.CreateAsync(factory, startInDays: 10);
        var guide = await factory.CreateClientAsAsync("guide1@tripcraft.test");
        var manager = await factory.CreateClientAsAsync("manager1@tripcraft.test");

        var created = await guide.PostAsJsonAsync($"/api/trip-requests/{trip.TripId}/guide-change-requests",
            new CreateGuideChangeRequest("Family wedding that week"));
        var again = await guide.PostAsJsonAsync($"/api/trip-requests/{trip.TripId}/guide-change-requests",
            new CreateGuideChangeRequest("Still busy"));
        var open = await manager.GetFromJsonAsync<List<GuideChangeRequestDto>>("/api/guide-change-requests", TestJson.Options);

        created.StatusCode.Should().Be(HttpStatusCode.Created);
        again.StatusCode.Should().Be(HttpStatusCode.Conflict, "one open request per trip");
        var request = open!.Should().ContainSingle(r => r.TripRequestId == trip.TripId).Subject;
        request.GuideName.Should().Be("Nimal Perera");
        request.Reason.Should().Be("Family wedding that week");
        request.Language.Should().Be("en");
        request.Candidates.Should().NotContain(c => c.Id == ResourcesSeeder.NimalGuide)
            .And.OnlyContain(c => c.Languages.Contains("en"))
            .And.Contain(c => c.Id == Kumari);

        var resolved = await manager.PostAsJsonAsync($"/api/guide-change-requests/{request.Id}/resolve",
            new ResolveGuideChangeRequest(Kumari));

        resolved.StatusCode.Should().Be(HttpStatusCode.OK);
        (await resolved.Content.ReadFromJsonAsync<GuideChangeRequestDto>(TestJson.Options))!.ReplacementGuideName.Should().Be("Kumari Silva");
        var (holds, notified) = await factory.QueryDbAsync(async db => (
            await db.ResourceHolds.Where(h => h.TripRequestId == trip.TripId && h.ResourceType == ResourceType.Guide).ToListAsync(),
            await db.Notifications.Where(n => n.TripRequestId == trip.TripId).Select(n => n.Type).ToListAsync()));
        holds.Should().ContainSingle(h => h.ResourceId == ResourcesSeeder.NimalGuide && h.Status == HoldStatus.Released);
        holds.Should().ContainSingle(h => h.ResourceId == Kumari && h.Status == HoldStatus.Held);
        notified.Should().Contain(["GuideChangeRequested", "GuideReplaced", "TripAssigned", "GuideChanged"]);
        // The new guide now has the trip; the old one does not.
        var kumari = await factory.CreateClientAsAsync("guide2@tripcraft.test");
        (await kumari.GetFromJsonAsync<GuideScheduleDto>("/api/guides/me/schedule", TestJson.Options))!
            .Trips.Should().Contain(t => t.TripRequestId == trip.TripId);
        (await guide.GetFromJsonAsync<GuideScheduleDto>("/api/guides/me/schedule", TestJson.Options))!
            .Trips.Should().NotContain(t => t.TripRequestId == trip.TripId);
    }

    [Fact]
    public async Task A_replacement_that_is_not_a_candidate_is_409_and_nothing_changes()
    {
        await using var factory = new RealComponentsFactory();
        var trip = await ConfirmedTrip.CreateAsync(factory, startInDays: 10);
        var guide = await factory.CreateClientAsAsync("guide1@tripcraft.test");
        var manager = await factory.CreateClientAsAsync("manager1@tripcraft.test");
        var request = (await (await guide.PostAsJsonAsync($"/api/trip-requests/{trip.TripId}/guide-change-requests",
            new CreateGuideChangeRequest("Sick"))).Content.ReadFromJsonAsync<GuideChangeRequestDto>(TestJson.Options))!;

        var response = await manager.PostAsJsonAsync($"/api/guide-change-requests/{request.Id}/resolve",
            new ResolveGuideChangeRequest(ResourcesSeeder.NimalGuide));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await factory.QueryDbAsync(db => db.ResourceHolds.CountAsync(h => h.TripRequestId == trip.TripId
            && h.ResourceType == ResourceType.Guide && h.Status == HoldStatus.Held && h.ResourceId == ResourcesSeeder.NimalGuide)))
            .Should().Be(1);
    }

    [Fact]
    public async Task Only_the_trips_guide_can_ask_and_only_managers_resolve()
    {
        await using var factory = new RealComponentsFactory();
        var trip = await ConfirmedTrip.CreateAsync(factory, startInDays: 10);
        var otherGuide = await factory.CreateClientAsAsync("guide2@tripcraft.test");
        var tourist = await factory.CreateClientAsAsync("tourist1@tripcraft.test");

        (await otherGuide.PostAsJsonAsync($"/api/trip-requests/{trip.TripId}/guide-change-requests", new CreateGuideChangeRequest("x")))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await tourist.PostAsJsonAsync($"/api/trip-requests/{trip.TripId}/guide-change-requests", new CreateGuideChangeRequest("x")))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await otherGuide.GetAsync("/api/guide-change-requests")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
