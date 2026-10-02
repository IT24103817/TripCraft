using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Quotations.Dtos;
using TripCraft.Application.Trips.Dtos;
using TripCraft.Application.Workflows.Dtos;
using TripCraft.Tests.Common;

namespace TripCraft.Tests.Workflows;

/// <summary>
/// "Edit &amp; resend" after the client accepted, and "Edit &amp; send manually" when the trip needs the operator (v1.1):
/// edit a day or swap a resource, Re-price (a new version, not sent), then Send. The client must accept the new
/// version again, and Confirm is refused until they do.
/// </summary>
public class ProposalEditTests
{
    [Fact]
    public async Task Edit_and_resend_needs_a_reprice_the_client_accepts_again_and_only_then_confirm_works()
    {
        await using var factory = new RealComponentsFactory();
        var (trip, outcome) = await factory.RunToClientAcceptedAsync();
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);
        var tourist = await factory.CreateClientAsAsync(WorkflowFlow.Tourist);
        // A Kandy attraction that is not in the golden proposal and costs a different entry fee than the temple.
        var palace = await factory.QueryDbAsync(db => db.Attractions.FirstAsync(a => a.City == "Kandy" && a.EntryFeeLkr != 2000
            && a.Name != "Temple of the Sacred Tooth Relic" && a.Name != "Royal Botanical Gardens, Peradeniya"));

        var edit = await manager.PutAsJsonAsync($"/api/trip-requests/{trip.Id}/proposal/days/1",
            new EditItineraryDayRequest([palace.Id], null));
        var confirmWhileEdited = await manager.PostAsync($"/api/trip-requests/{trip.Id}/confirm", null);
        var sendTooEarly = await manager.PostAsync($"/api/quotations/{outcome.QuotationId}/send", null);
        var reprice = await manager.PostAsync($"/api/quotations/{outcome.QuotationId}/calculate", null);

        edit.StatusCode.Should().Be(HttpStatusCode.OK);
        (await edit.Content.ReadFromJsonAsync<EditableProposalDto>(TestJson.Options))!.EditedSinceQuotation.Should().BeTrue();
        confirmWhileEdited.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await confirmWhileEdited.Content.ReadAsStringAsync()).Should().Contain("must accept again");
        sendTooEarly.StatusCode.Should().Be(HttpStatusCode.Conflict, "version 1 was already sent");
        reprice.StatusCode.Should().Be(HttpStatusCode.OK);
        var v2 = (await reprice.Content.ReadFromJsonAsync<RepriceResponse>(TestJson.Options))!;
        v2.Version.Should().Be(2);
        v2.TotalLkr.Should().NotBe(v2.PreviousTotalLkr, "another entry fee is priced");

        // Version 2 exists but is not sent or accepted yet: Confirm is still refused.
        (await manager.PostAsync($"/api/trip-requests/{trip.Id}/confirm", null)).StatusCode.Should().Be(HttpStatusCode.Conflict);

        var send = await manager.PostAsync($"/api/quotations/{v2.QuotationId}/send", null);
        send.StatusCode.Should().Be(HttpStatusCode.OK);
        (await send.Content.ReadFromJsonAsync<Application.Quotations.QuotationDecisionResponse>(TestJson.Options))!
            .TripStatus.Should().Be("QuotationSent");
        (await factory.QueryDbAsync(db => db.Notifications.AnyAsync(n => n.Type == "QuotationUpdated" && n.TripRequestId == trip.Id)))
            .Should().BeTrue("the tourist is told the quote was updated");
        (await manager.PostAsync($"/api/trip-requests/{trip.Id}/confirm", null)).StatusCode.Should().Be(HttpStatusCode.Conflict);

        (await tourist.PostAsync($"/api/quotations/{v2.QuotationId}/accept", null)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await manager.PostAsync($"/api/trip-requests/{trip.Id}/confirm", null)).StatusCode.Should().Be(HttpStatusCode.OK);

        // The review page compares the snapshots of v1 and v2.
        var versions = await manager.GetFromJsonAsync<Application.Common.Paging.PagedResult<QuotationDto>>(
            $"/api/quotations?tripRequestId={trip.Id}&sort=version", TestJson.Options);
        versions!.Items.Select(q => q.Status).Should().Equal("Superseded", "Approved");
        versions.Items[0].ProposalSnapshot!.Value.GetRawText().Should().NotContain(palace.Id.ToString());
        versions.Items[1].ProposalSnapshot!.Value.GetRawText().Should().Contain(palace.Id.ToString());
    }

    [Fact]
    public async Task Edit_and_send_manually_fixes_a_proposal_that_failed_a_hard_rule()
    {
        await using var factory = new RealComponentsFactory();
        var (trip, workflowId) = await factory.StartPlanningAsync();
        var proposal = TestProposals.Golden(trip.StartDate, await factory.SeededAttractionsAsync());
        var car = Guid.Parse("00000000-0000-0000-0000-00000000b002"); // 3 seats for 4 people: VEHICLE_SEATS
        proposal = proposal with { Resources = proposal.Resources! with { VehicleId = car.ToString() } };
        (await factory.PostProposalAsync(workflowId, proposal)).EnsureSuccessStatusCode();
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);
        (await factory.QueryDbAsync(db => db.TripRequests.SingleAsync(t => t.Id == trip.Id))).Status
            .Should().Be(Application.Trips.TripRequestStatus.NeedsOperator);

        var van = Guid.Parse("00000000-0000-0000-0000-00000000b001");
        (await manager.PutAsJsonAsync($"/api/trip-requests/{trip.Id}/proposal/resources", new SwapResourcesRequest(null, van, null)))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        var reprice = await manager.PostAsync($"/api/trip-requests/{trip.Id}/proposal/reprice", null); // no quotation yet
        reprice.StatusCode.Should().Be(HttpStatusCode.OK);
        var v1 = (await reprice.Content.ReadFromJsonAsync<RepriceResponse>(TestJson.Options))!;
        var send = await manager.PostAsync($"/api/quotations/{v1.QuotationId}/send", null);

        v1.Version.Should().Be(1);
        v1.Validation.HasHard.Should().BeFalse();
        send.StatusCode.Should().Be(HttpStatusCode.OK);
        (await factory.QueryDbAsync(db => db.TripRequests.SingleAsync(t => t.Id == trip.Id))).Status
            .Should().Be(Application.Trips.TripRequestStatus.QuotationSent);
    }

    [Fact]
    public async Task A_stop_in_another_city_is_400()
    {
        await using var factory = new RealComponentsFactory();
        var (trip, _) = await factory.RunToClientAcceptedAsync();
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
        var (trip, outcome) = await factory.RunToClientAcceptedAsync();
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
    public async Task Nothing_is_edited_while_the_client_is_deciding()
    {
        await using var factory = new RealComponentsFactory();
        var (trip, _) = await factory.RunToProposalAsync(); // QuotationSent
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);
        var temple = await factory.QueryDbAsync(db => db.Attractions.FirstAsync(a => a.City == "Kandy"));

        var response = await manager.PutAsJsonAsync($"/api/trip-requests/{trip.Id}/proposal/days/1",
            new EditItineraryDayRequest([temple.Id], null));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await response.Content.ReadAsStringAsync()).Should().Contain("quotation sent");
    }
}
