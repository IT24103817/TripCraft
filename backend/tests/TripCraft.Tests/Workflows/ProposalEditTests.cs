using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Quotations.Dtos;
using TripCraft.Application.Trips.Dtos;
using TripCraft.Application.Workflows.Dtos;
using TripCraft.Tests.Common;

namespace TripCraft.Tests.Workflows;

/// <summary>"Edit directly" at PendingReview: edit a day or swap a resource, then Re-price, then Send (v1.1).</summary>
public class ProposalEditTests
{
    [Fact]
    public async Task An_edited_day_must_be_repriced_and_the_new_version_can_then_be_sent()
    {
        await using var factory = new RealComponentsFactory();
        var (trip, outcome) = await factory.RunToProposalAsync();
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);
        // A Kandy attraction that is not in the golden proposal and costs a different entry fee than the temple.
        var palace = await factory.QueryDbAsync(db => db.Attractions.FirstAsync(a => a.City == "Kandy" && a.EntryFeeLkr != 2000
            && a.Name != "Temple of the Sacred Tooth Relic" && a.Name != "Royal Botanical Gardens, Peradeniya"));

        var edit = await manager.PutAsJsonAsync($"/api/trip-requests/{trip.Id}/proposal/days/1",
            new EditItineraryDayRequest([palace.Id], null));
        var sendTooEarly = await manager.PostAsync($"/api/quotations/{outcome.QuotationId}/approve", null);
        var reprice = await manager.PostAsync($"/api/quotations/{outcome.QuotationId}/calculate", null);

        edit.StatusCode.Should().Be(HttpStatusCode.OK);
        var edited = (await edit.Content.ReadFromJsonAsync<EditableProposalDto>(TestJson.Options))!;
        edited.EditedSinceQuotation.Should().BeTrue();
        edited.Days[0].Stops!.Should().ContainSingle(s => s.Name == palace.Name);
        sendTooEarly.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await sendTooEarly.Content.ReadAsStringAsync()).Should().Contain("Re-price");
        reprice.StatusCode.Should().Be(HttpStatusCode.OK);
        var v2 = (await reprice.Content.ReadFromJsonAsync<RepriceResponse>(TestJson.Options))!;
        v2.Version.Should().Be(2);
        v2.TotalLkr.Should().NotBe(v2.PreviousTotalLkr, "another entry fee is priced");
        (await manager.PostAsync($"/api/quotations/{v2.QuotationId}/approve", null)).StatusCode.Should().Be(HttpStatusCode.OK);

        // The review page compares the snapshots of v1 and v2.
        var versions = await manager.GetFromJsonAsync<Application.Common.Paging.PagedResult<QuotationDto>>(
            $"/api/quotations?tripRequestId={trip.Id}&sort=version", TestJson.Options);
        versions!.Items.Select(q => q.Status).Should().Equal("Superseded", "Approved");
        versions.Items[0].ProposalSnapshot!.Value.GetRawText().Should().NotContain(palace.Id.ToString());
        versions.Items[1].ProposalSnapshot!.Value.GetRawText().Should().Contain(palace.Id.ToString());
    }

    [Fact]
    public async Task A_stop_in_another_city_is_400()
    {
        await using var factory = new RealComponentsFactory();
        var (trip, _) = await factory.RunToProposalAsync();
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);
        var galle = await factory.QueryDbAsync(db => db.Attractions.FirstAsync(a => a.City == "Galle"));

        var response = await manager.PutAsJsonAsync($"/api/trip-requests/{trip.Id}/proposal/days/1",
            new EditItineraryDayRequest([galle.Id], null));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain("not Kandy");
    }

    [Fact]
    public async Task Swapping_to_a_free_guide_and_vehicle_works_and_an_unsuitable_one_is_409()
    {
        await using var factory = new RealComponentsFactory();
        var (trip, outcome) = await factory.RunToProposalAsync();
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);
        var kumari = Guid.Parse("00000000-0000-0000-0000-00000000a002");
        var coach = Guid.Parse("00000000-0000-0000-0000-00000000b003");
        var smallCar = Guid.Parse("00000000-0000-0000-0000-00000000b002");

        var tooSmall = await manager.PutAsJsonAsync($"/api/trip-requests/{trip.Id}/proposal/resources",
            new SwapResourcesRequest(null, smallCar, null));
        var swap = await manager.PutAsJsonAsync($"/api/trip-requests/{trip.Id}/proposal/resources",
            new SwapResourcesRequest(kumari, coach, null));
        var nothing = await manager.PutAsJsonAsync($"/api/trip-requests/{trip.Id}/proposal/resources",
            new SwapResourcesRequest(null, null, null));

        tooSmall.StatusCode.Should().Be(HttpStatusCode.Conflict, "a 3-seat car cannot take 4 people");
        swap.StatusCode.Should().Be(HttpStatusCode.OK);
        var swapped = (await swap.Content.ReadFromJsonAsync<EditableProposalDto>(TestJson.Options))!;
        swapped.Resources!.GuideId.Should().Be(kumari.ToString());
        swapped.Resources.VehicleId.Should().Be(coach.ToString());
        nothing.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var v2 = await (await manager.PostAsync($"/api/quotations/{outcome.QuotationId}/calculate", null))
            .Content.ReadFromJsonAsync<RepriceResponse>(TestJson.Options);
        v2!.TotalLkr.Should().BeGreaterThan(v2.PreviousTotalLkr, "Kumari and the coach cost more");
    }

    [Fact]
    public async Task Editing_is_only_possible_while_the_trip_is_in_review()
    {
        await using var factory = new RealComponentsFactory();
        var (trip, outcome) = await factory.RunToProposalAsync();
        await factory.SendToClientAsync(outcome.QuotationId!.Value);
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);
        var temple = await factory.QueryDbAsync(db => db.Attractions.FirstAsync(a => a.City == "Kandy"));

        var response = await manager.PutAsJsonAsync($"/api/trip-requests/{trip.Id}/proposal/days/1",
            new EditItineraryDayRequest([temple.Id], null));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await response.Content.ReadAsStringAsync()).Should().Contain("quotation sent");
    }
}
