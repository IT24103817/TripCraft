using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TripCraft.Application.Quotations;
using TripCraft.Application.Quotations.Dashboard;
using TripCraft.Application.Quotations.Dtos;
using TripCraft.Application.Workflows.Services;
using TripCraft.Tests.Common;
using TripCraft.Tests.Resources;
using TripCraft.Tests.Workflows;

namespace TripCraft.Tests.Quotations;

/// <summary>Manager dashboard counts and trips, "Why this plan", the deposit flag and the itinerary PDF (v1.1).</summary>
public class DashboardAndDocumentsTests
{
    [Fact]
    public async Task Action_counts_follow_the_lifecycle()
    {
        await using var factory = new RealComponentsFactory();
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);
        var (sent, _) = await factory.RunToProposalAsync();                  // auto-sent: nothing for the operator
        var (accepted, _) = await factory.RunToClientAcceptedAsync();        // a trip to confirm
        var (declinedTrip, declined) = await factory.RunToProposalAsync();   // sent, then declined
        var tourist = await factory.CreateClientAsAsync(WorkflowFlow.Tourist);
        await tourist.PostAsJsonAsync($"/api/quotations/{declined.QuotationId}/decline", new DeclineQuotationRequest("Too long"));

        var actions = await manager.GetFromJsonAsync<DashboardActionsDto>("/api/dashboard/actions", TestJson.Options);
        var attention = await manager.GetFromJsonAsync<List<AttentionItemDto>>("/api/dashboard/attention", TestJson.Options);

        actions!.AcceptedToConfirm.Should().Be(1);
        actions.DeclinedNeedsDecision.Should().Be(1);
        actions.NeedsOperator.Should().Be(0);
        actions.GuideChangeRequests.Should().Be(0);
        attention!.Select(a => (a.TripRequestId, a.Status, a.Detail)).Should().BeEquivalentTo(new[]
        {
            (accepted.Id, "ClientAccepted", "Version 1 accepted"),
            (declinedTrip.Id, "ClientDeclined", "Too long")
        });
        attention.Should().NotContain(a => a.TripRequestId == sent.Id);
    }

    [Fact]
    public async Task Upcoming_lists_trips_running_today_with_guide_and_vehicle_and_tourists_get_403()
    {
        await using var factory = new TestWebApplicationFactory();
        var trip = await ConfirmedTrip.CreateAsync(factory);
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);
        var tourist = await factory.CreateClientAsAsync(WorkflowFlow.Tourist);

        var upcoming = await manager.GetFromJsonAsync<List<UpcomingTripDto>>("/api/dashboard/upcoming", TestJson.Options);

        var row = upcoming!.Should().ContainSingle(u => u.TripRequestId == trip.TripId).Subject;
        row.Day.Should().Be("today");
        row.DayNumber.Should().Be(1);
        row.GuideName.Should().Be("Nimal Perera");
        row.VehicleRegistrationNo.Should().Be("CAB-1234");
        row.TouristName.Should().Be("Demo Tourist 1");
        (await tourist.GetAsync("/api/dashboard/actions")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Why_this_plan_explains_guide_vehicle_hotels_driving_and_budget()
    {
        await using var factory = new RealComponentsFactory();
        var (trip, _) = await factory.RunToProposalAsync();
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);

        var explanation = await manager.GetFromJsonAsync<PlanExplanationDto>($"/api/trip-requests/{trip.Id}/plan-explanation", TestJson.Options);

        explanation!.Items.Select(i => i.Topic).Should().Equal("guide", "vehicle", "hotels", "driving", "budget");
        explanation.Items[0].Title.Should().Be("Guide: Nimal Perera");
        explanation.Items[1].Text.Should().Contain("6 seats for 4 travellers");
        explanation.Items[2].Text.Should().Contain("Kandy Hills").And.Contain("Ella Gap");
        explanation.Items[3].Text.Should().Contain("Day 3 to Ella: 140 km by train");
        explanation.Items[4].Text.Should().Contain("under budget");
        (await manager.GetAsync($"/api/trip-requests/{Guid.NewGuid()}/plan-explanation")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public void Budget_sentence_says_how_far_under_or_over_the_total_is()
    {
        var trip = new Application.Trips.TripRequest { BudgetUsd = 1000, Pax = 2 };

        PlanExplanationService.Budget(trip, 600).Text.Should().Contain("40% under budget");
        PlanExplanationService.Budget(trip, 1250).Text.Should().Contain("over the budget of USD 1,000.00 by 25%");
    }

    [Fact]
    public async Task The_deposit_can_be_marked_paid_only_after_the_client_accepted()
    {
        await using var factory = new RealComponentsFactory();
        var (_, outcome) = await factory.RunToProposalAsync();
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);

        var tooEarly = await manager.PostAsJsonAsync($"/api/quotations/{outcome.QuotationId}/payment", new SetPaymentRequest(true));
        var tourist = await factory.CreateClientAsAsync(WorkflowFlow.Tourist);
        await tourist.PostAsync($"/api/quotations/{outcome.QuotationId}/accept", null);
        var paid = await manager.PostAsJsonAsync($"/api/quotations/{outcome.QuotationId}/payment", new SetPaymentRequest(true));
        var unpaid = await manager.PostAsJsonAsync($"/api/quotations/{outcome.QuotationId}/payment", new SetPaymentRequest(false));

        tooEarly.StatusCode.Should().Be(HttpStatusCode.Conflict);
        paid.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = (await paid.Content.ReadFromJsonAsync<QuotationDto>(TestJson.Options))!;
        dto.DepositPaid.Should().BeTrue();
        dto.DepositPct.Should().Be(30);
        (await unpaid.Content.ReadFromJsonAsync<QuotationDto>(TestJson.Options))!.DepositPaid.Should().BeFalse();
        (await tourist.PostAsJsonAsync($"/api/quotations/{outcome.QuotationId}/payment", new SetPaymentRequest(true)))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task The_itinerary_pdf_is_available_once_a_quotation_was_sent_to_its_owner_and_managers()
    {
        await using var factory = new RealComponentsFactory();
        var (trip, workflowId) = await factory.StartPlanningAsync();
        var owner = await factory.CreateClientAsAsync(WorkflowFlow.Tourist);
        var other = await factory.CreateClientAsAsync(WorkflowFlow.OtherTourist);

        var tooEarly = await owner.GetAsync($"/api/trips/{trip.Id}/itinerary.pdf"); // still planning
        (await factory.PostProposalAsync(workflowId, TestProposals.Golden(trip.StartDate, await factory.SeededAttractionsAsync())))
            .EnsureSuccessStatusCode(); // auto-sent
        var pdf = await owner.GetAsync($"/api/trips/{trip.Id}/itinerary.pdf");

        tooEarly.StatusCode.Should().Be(HttpStatusCode.Conflict);
        pdf.StatusCode.Should().Be(HttpStatusCode.OK);
        pdf.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
        System.Text.Encoding.ASCII.GetString(await pdf.Content.ReadAsByteArrayAsync(), 0, 5).Should().Be("%PDF-");
        (await other.GetAsync($"/api/trips/{trip.Id}/itinerary.pdf")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
