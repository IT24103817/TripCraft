using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Quotations;
using TripCraft.Application.Trips;
using TripCraft.Application.Trips.Dtos;
using TripCraft.Application.Workflows;
using TripCraft.Application.Workflows.Dtos;
using TripCraft.Application.Workflows.Ports;
using TripCraft.Tests.Common;
using TripCraft.Tests.Workflows;
using TripCraft.Tests.Workflows.Fakes;

namespace TripCraft.Tests.Quotations;

/// <summary>
/// The v1.1 lifecycle through the API: the agents' proposal goes straight to the client (QuotationSent); the client
/// accepts or declines; the Operations Manager confirms (the human approval gate, the only place holds are made),
/// replans after a decline, or handles NeedsOperator. Each test gets its own app because Confirm creates holds.
/// </summary>
public class QuotationApprovalTests
{
    [Fact]
    public async Task A_valid_proposal_is_sent_to_the_client_automatically_with_no_holds()
    {
        await using var factory = new TestWebApplicationFactory();

        var (trip, outcome) = await factory.RunToProposalAsync();

        outcome.Status.Should().Be("Approved");
        var quotation = factory.State<FakeQuotationsState>().Quotations.Single(q => q.Id == outcome.QuotationId);
        quotation.Status.Should().Be("Approved"); // = sent to the client
        quotation.Draft.OverBudgetUsd.Should().BeNull();
        factory.State<FakeQuotationsState>().Decisions.Should().BeEmpty("no manager decided anything");
        factory.State<FakeResourcesState>().Holds.Should().BeEmpty("nothing is held before Confirm");
        var (status, notified, emails) = await factory.QueryDbAsync(async db => (
            (await db.TripRequests.SingleAsync(t => t.Id == trip.Id)).Status,
            await db.Notifications.AnyAsync(n => n.Type == "QuotationSent" && n.TripRequestId == trip.Id),
            await db.EmailOutbox.CountAsync(e => e.TripRequestId == trip.Id && e.SentAt != null)));
        status.Should().Be(TripRequestStatus.QuotationSent);
        notified.Should().BeTrue();
        emails.Should().Be(1);
    }

    [Fact]
    public async Task Still_over_budget_after_the_replans_it_is_sent_anyway_as_the_best_available_price()
    {
        await using var factory = new RealComponentsFactory();

        var (trip, outcome) = await factory.RunToProposalAsync(budgetUsd: 400);

        outcome.Status.Should().Be("Approved");
        outcome.Validation.Violations.Should().ContainSingle(v => v.Code == "OVER_BUDGET" && v.Severity == ViolationSeverity.Soft);
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);
        var quotation = await manager.GetFromJsonAsync<Application.Quotations.Dtos.QuotationDto>(
            $"/api/quotations/{outcome.QuotationId}", TestJson.Options);
        quotation!.Status.Should().Be("Approved");
        quotation.BestAvailablePrice.Should().BeTrue();
        quotation.OverBudgetUsd.Should().Be(quotation.TotalUsd - 400);
        quotation.BudgetNote.Should().Be($"Best price we can offer — USD {quotation.TotalUsd - 400:N2} above your budget");
        (await factory.QueryDbAsync(db => db.TripRequests.SingleAsync(t => t.Id == trip.Id))).Status
            .Should().Be(TripRequestStatus.QuotationSent);
    }

    [Fact]
    public async Task A_hard_rule_is_never_sent_the_trip_needs_the_operator_who_retries_planning()
    {
        await using var factory = new TestWebApplicationFactory();
        var (trip, workflowId) = await factory.StartPlanningAsync();
        var bad = TestProposals.Golden(trip.StartDate, await factory.SeededAttractionsAsync());
        bad.Days![0] = bad.Days[0] with { DrivingMinutes = 600 }; // DRIVING_LIMIT is a Hard rule

        var outcome = (await (await factory.PostProposalAsync(workflowId, bad)).Content
            .ReadFromJsonAsync<ProposalOutcomeResponse>(TestJson.Options))!;

        outcome.Status.Should().Be("FailedSafely");
        outcome.QuotationId.Should().BeNull();
        factory.State<FakeQuotationsState>().Quotations.Should().BeEmpty("a quote that failed a Hard rule is never sent");
        var (status, managersTold, touristTold) = await factory.QueryDbAsync(async db => (
            (await db.TripRequests.SingleAsync(t => t.Id == trip.Id)).Status,
            await db.Notifications.AnyAsync(n => n.Type == "NeedsOperator" && n.TripRequestId == trip.Id),
            await db.Notifications.AnyAsync(n => n.Type == "QuotationSent" && n.TripRequestId == trip.Id)));
        status.Should().Be(TripRequestStatus.NeedsOperator);
        managersTold.Should().BeTrue();
        touristTold.Should().BeFalse();

        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);
        var attention = await manager.GetFromJsonAsync<List<Application.Quotations.Dashboard.AttentionItemDto>>(
            "/api/dashboard/attention?status=NeedsOperator", TestJson.Options);
        attention!.Should().ContainSingle(a => a.TripRequestId == trip.Id).Which.Detail.Should().Contain("DRIVING_LIMIT");

        var retry = await manager.PostAsync($"/api/trip-requests/{trip.Id}/start-planning", null);
        retry.StatusCode.Should().Be(HttpStatusCode.Accepted);
        (await retry.Content.ReadFromJsonAsync<StartPlanningResponse>(TestJson.Options))!.TripStatus.Should().Be("Planning");
    }

    [Fact]
    public async Task Tourist_accepts_then_the_manager_confirms_with_holds_itinerary_vouchers_and_emails()
    {
        await using var factory = new TestWebApplicationFactory();
        var (trip, outcome) = await factory.RunToClientAcceptedAsync();
        var managerNotified = await factory.QueryDbAsync(db => db.Notifications.AnyAsync(n => n.Type == "ClientAccepted"));
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);

        var response = await manager.PostAsync($"/api/trip-requests/{trip.Id}/confirm", null);

        managerNotified.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var decision = (await response.Content.ReadFromJsonAsync<QuotationDecisionResponse>(TestJson.Options))!;
        decision.TripStatus.Should().Be("Confirmed");
        decision.WorkflowStatus.Should().Be("Completed");
        decision.HoldsCreated.Should().Be(6);
        factory.State<FakeResourcesState>().Holds.Should().HaveCount(6).And.OnlyContain(h => h.TripRequestId == trip.Id);
        factory.State<FakeQuotationsState>().Decisions.Select(d => d.Decision)
            .Should().Equal(QuotationDecision.Accepted, QuotationDecision.Confirmed);

        var (vouchers, emails) = await factory.QueryDbAsync(async db => (
            await db.Vouchers.CountAsync(v => v.TripRequestId == trip.Id),
            await db.EmailOutbox.Where(e => e.TripRequestId == trip.Id).OrderBy(e => e.CreatedAt).Select(e => e.Subject).ToListAsync()));
        vouchers.Should().Be(5);
        emails.Should().Equal("Your TripCraft quotation is ready", "Your TripCraft trip is confirmed");
        outcome.QuotationId.Should().NotBeNull();
    }

    [Fact]
    public async Task Every_status_change_is_in_the_trip_history_with_actor_and_reason()
    {
        await using var factory = new TestWebApplicationFactory();
        var (trip, _) = await factory.RunToConfirmedAsync();
        var tourist = await factory.CreateClientAsAsync(WorkflowFlow.Tourist);

        var history = await tourist.GetFromJsonAsync<List<TripHistoryEntryDto>>($"/api/trip-requests/{trip.Id}/history", TestJson.Options);

        var changes = history!.Where(h => h.Action == "TripRequestStatusChanged").ToList();
        changes.Select(h => h.ToStatus).Should().Equal("Planning", "QuotationSent", "ClientAccepted", "Confirmed");
        changes.Select(h => h.Actor).Should().Equal("Tourist", "System", "Tourist", "OperationsManager");
        changes.Should().OnlyContain(h => !string.IsNullOrEmpty(h.Reason));
    }

    [Fact]
    public async Task A_declined_quote_waits_for_the_operator_who_replans_with_a_note_and_version_2_is_sent()
    {
        await using var factory = new RealComponentsFactory();
        var (trip, outcome) = await factory.RunToProposalAsync();
        var tourist = await factory.CreateClientAsAsync(WorkflowFlow.Tourist);
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);

        var noReason = await tourist.PostAsJsonAsync($"/api/quotations/{outcome.QuotationId}/decline", new DeclineQuotationRequest(""));
        var declined = await tourist.PostAsJsonAsync($"/api/quotations/{outcome.QuotationId}/decline", new DeclineQuotationRequest("Too many temples"));

        noReason.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await declined.Content.ReadFromJsonAsync<QuotationDecisionResponse>(TestJson.Options))!.TripStatus.Should().Be("ClientDeclined");
        var attention = await manager.GetFromJsonAsync<List<Application.Quotations.Dashboard.AttentionItemDto>>(
            "/api/dashboard/attention?status=ClientDeclined", TestJson.Options);
        attention!.Should().ContainSingle(a => a.TripRequestId == trip.Id).Which.Detail.Should().Be("Too many temples");
        (await factory.QueryDbAsync(db => db.Notifications.AnyAsync(n => n.Type == "ClientDeclined" && n.TripRequestId == trip.Id)))
            .Should().BeTrue("the managers are told");
        (await manager.PostAsync($"/api/trip-requests/{trip.Id}/confirm", null)).StatusCode.Should().Be(HttpStatusCode.Conflict);

        var noNote = await manager.PostAsJsonAsync($"/api/trip-requests/{trip.Id}/replan", new ReplanRequest(""));
        var replan = await manager.PostAsJsonAsync($"/api/trip-requests/{trip.Id}/replan", new ReplanRequest("Swap a temple for the lake"));

        noNote.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await replan.Content.ReadFromJsonAsync<QuotationDecisionResponse>(TestJson.Options))!.TripStatus.Should().Be("Planning");
        factory.State<FakeAgentState>().Calls.Should().Contain(c => c.Kind == "replan" && c.WorkflowId == outcome.WorkflowId
            && c.Comment == "Swap a temple for the lake\nThe client declined the last quote: Too many temples");

        // The Planner's new proposal is sent to the client automatically as version 2.
        var replanned = TestProposals.Golden(trip.StartDate, await factory.SeededAttractionsAsync());
        (await factory.PostProposalAsync(outcome.WorkflowId, replanned)).EnsureSuccessStatusCode();
        var versions = await factory.QueryDbAsync(db => db.Quotations.Where(q => q.TripRequestId == trip.Id)
            .OrderBy(q => q.Version).Select(q => new { q.Version, q.Status }).ToListAsync());
        versions.Select(v => (v.Version, v.Status.ToString())).Should().Equal((1, "Declined"), (2, "Approved"));
        (await factory.QueryDbAsync(db => db.TripRequests.SingleAsync(t => t.Id == trip.Id))).Status
            .Should().Be(TripRequestStatus.QuotationSent);
    }

    [Fact]
    public async Task The_manager_can_cancel_a_declined_trip_with_a_reason_and_the_tourist_is_told()
    {
        await using var factory = new TestWebApplicationFactory();
        var (trip, outcome) = await factory.RunToProposalAsync();
        var tourist = await factory.CreateClientAsAsync(WorkflowFlow.Tourist);
        await tourist.PostAsJsonAsync($"/api/quotations/{outcome.QuotationId}/decline", new DeclineQuotationRequest("Dates changed"));
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);

        var cancel = await manager.PostAsJsonAsync($"/api/trip-requests/{trip.Id}/cancel", new CancelTripRequest("Client travels next year"));

        (await cancel.Content.ReadFromJsonAsync<TripRequestDto>(TestJson.Options))!.Status.Should().Be("Cancelled");
        (await factory.QueryDbAsync(db => db.Notifications.AnyAsync(n => n.Type == "TripCancelled" && n.TripRequestId == trip.Id)))
            .Should().BeTrue();
    }

    [Fact]
    public async Task Confirm_with_a_conflicting_hold_returns_409_rolls_back_and_the_trip_stays_ClientAccepted()
    {
        await using var factory = new TestWebApplicationFactory();
        var (trip, _) = await factory.RunToClientAcceptedAsync();
        var resources = factory.State<FakeResourcesState>();
        resources.Holds.Add(new ResourceHoldRequest(ResourceType.Vehicle, FakeResourcesState.VanSixSeats, Guid.NewGuid(),
            trip.StartDate.AddDays(1), trip.StartDate.AddDays(2), 1));
        var before = await CountRowsAsync(factory);
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);

        var response = await manager.PostAsync($"/api/trip-requests/{trip.Id}/confirm", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        (await CountRowsAsync(factory)).Should().Be(before);
        resources.Holds.Should().ContainSingle();
        (await factory.QueryDbAsync(db => db.TripRequests.SingleAsync(t => t.Id == trip.Id))).Status
            .Should().Be(TripRequestStatus.ClientAccepted);
    }

    [Fact]
    public async Task Old_review_endpoints_are_gone()
    {
        await using var factory = new TestWebApplicationFactory();
        var (trip, outcome) = await factory.RunToProposalAsync();
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);

        (await manager.PostAsync($"/api/quotations/{outcome.QuotationId}/approve", null)).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await manager.PostAsync($"/api/quotations/{outcome.QuotationId}/request-revision", null)).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await manager.PostAsync($"/api/quotations/{outcome.QuotationId}/reject", null)).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await manager.PostAsync($"/api/trip-requests/{trip.Id}/reopen-review", null)).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("tourist1@tripcraft.test")]
    [InlineData("admin1@tripcraft.test")]
    public async Task Only_an_operations_manager_confirms_sends_or_replans(string email)
    {
        await using var factory = new TestWebApplicationFactory();
        var (trip, outcome) = await factory.RunToClientAcceptedAsync();
        var client = await factory.CreateClientAsAsync(email);

        (await client.PostAsync($"/api/trip-requests/{trip.Id}/confirm", null)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await client.PostAsync($"/api/quotations/{outcome.QuotationId}/send", null)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await client.PostAsJsonAsync($"/api/trip-requests/{trip.Id}/replan", new ReplanRequest("x"))).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        factory.State<FakeResourcesState>().Holds.Should().BeEmpty();
    }

    [Fact]
    public async Task Only_the_owner_tourist_can_accept()
    {
        await using var factory = new TestWebApplicationFactory();
        var (_, outcome) = await factory.RunToProposalAsync();
        var other = await factory.CreateClientAsAsync(WorkflowFlow.OtherTourist);
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);

        (await other.PostAsync($"/api/quotations/{outcome.QuotationId}/accept", null)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await manager.PostAsync($"/api/quotations/{outcome.QuotationId}/accept", null)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private static Task<(int Audit, int Workflows, int Vouchers, int Trips)> CountRowsAsync(TestWebApplicationFactory factory) =>
        factory.QueryDbAsync(async db => (
            await db.AuditLogs.CountAsync(), await db.AgentWorkflows.CountAsync(),
            await db.Vouchers.CountAsync(), await db.TripRequests.CountAsync()));
}
