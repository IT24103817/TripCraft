using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Quotations;
using TripCraft.Application.Trips;
using TripCraft.Application.Trips.Dtos;
using TripCraft.Application.Workflows;
using TripCraft.Application.Workflows.Ports;
using TripCraft.Tests.Common;
using TripCraft.Tests.Workflows;
using TripCraft.Tests.Workflows.Fakes;

namespace TripCraft.Tests.Quotations;

/// <summary>
/// The v1.1 lifecycle through the API: PendingReview → (manager) QuotationSent → (tourist) ClientAccepted or back to
/// PendingReview → (manager) Confirmed. Each test gets its own app, because Confirm creates holds that would
/// overlap with the next test's trip dates.
/// </summary>
public class QuotationApprovalTests
{
    [Fact]
    public async Task Send_to_client_places_no_holds_and_notifies_the_tourist()
    {
        await using var factory = new TestWebApplicationFactory();
        var (trip, outcome) = await factory.RunToProposalAsync();
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);

        var response = await manager.PostAsJsonAsync($"/api/quotations/{outcome.QuotationId}/approve",
            new QuotationDecisionRequest("Looks good"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var decision = (await response.Content.ReadFromJsonAsync<QuotationDecisionResponse>(TestJson.Options))!;
        decision.TripStatus.Should().Be("QuotationSent");
        decision.WorkflowStatus.Should().Be("Approved");
        decision.HoldsCreated.Should().Be(0);
        factory.State<FakeResourcesState>().Holds.Should().BeEmpty();
        factory.State<FakeQuotationsState>().Decisions.Should().ContainSingle(d => d.Decision == QuotationDecision.Approved && d.Comment == "Looks good");
        var notified = await factory.QueryDbAsync(db => db.Notifications.AnyAsync(n => n.Type == "QuotationSent" && n.TripRequestId == trip.Id));
        notified.Should().BeTrue();
    }

    [Fact]
    public async Task Tourist_accepts_then_the_manager_confirms_with_holds_itinerary_vouchers_and_an_email()
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
        decision.HoldsCreated.Should().Be(6); // guide + vehicle + 4 nights x 1 room type (2 rooms each)

        var holds = factory.State<FakeResourcesState>().Holds;
        holds.Should().HaveCount(6).And.OnlyContain(h => h.TripRequestId == trip.Id);
        holds.Where(h => h.Type == ResourceType.Room).Should().OnlyContain(h => h.Quantity == 2);
        factory.State<FakeQuotationsState>().Decisions.Select(d => d.Decision)
            .Should().Equal(QuotationDecision.Approved, QuotationDecision.Accepted, QuotationDecision.Confirmed);

        var (workflow, vouchers, emails, audit) = await factory.QueryDbAsync(async db => (
            await db.AgentWorkflows.SingleAsync(w => w.Id == outcome.WorkflowId),
            await db.Vouchers.Where(v => v.TripRequestId == trip.Id).ToListAsync(),
            await db.EmailOutbox.Where(e => e.TripRequestId == trip.Id).OrderBy(e => e.CreatedAt).ToListAsync(),
            await db.AuditLogs.AnyAsync(a => a.Action == "TripConfirmed" && a.EntityId == trip.Id)));
        workflow.FinalOutcome.Should().Contain("\"decision\":\"Confirmed\"");
        vouchers.Should().HaveCount(5); // 1 trip voucher + 4 hotel nights
        vouchers.Count(v => v.Type == Application.Vouchers.VoucherType.Trip).Should().Be(1);
        // One email when the quotation was sent, one when the trip was confirmed (Mailtrap is not set: pickup folder).
        emails.Select(e => e.Subject).Should().Equal("Your TripCraft quotation is ready", "Your TripCraft trip is confirmed");
        emails.Should().OnlyContain(e => e.SentAt != null);
        Directory.GetFiles(factory.MailDir, "*.eml").Should().HaveCount(2);
        audit.Should().BeTrue();

        // Step 11: the tourist now sees the saved itinerary (5 days) instead of the proposal.
        var tourist = await factory.CreateClientAsAsync(WorkflowFlow.Tourist);
        var itinerary = await tourist.GetFromJsonAsync<ItineraryDto>($"/api/trip-requests/{trip.Id}/itinerary", TestJson.Options);
        itinerary!.Days.Should().HaveCount(5);
    }

    [Fact]
    public async Task Every_status_change_is_in_the_trip_history_with_actor_and_reason()
    {
        await using var factory = new TestWebApplicationFactory();
        var (trip, _) = await factory.RunToConfirmedAsync();
        var tourist = await factory.CreateClientAsAsync(WorkflowFlow.Tourist);

        var history = await tourist.GetFromJsonAsync<List<TripHistoryEntryDto>>($"/api/trip-requests/{trip.Id}/history", TestJson.Options);

        var changes = history!.Where(h => h.Action == "TripRequestStatusChanged").ToList();
        changes.Select(h => h.ToStatus).Should().Equal("Planning", "PendingReview", "QuotationSent", "ClientAccepted", "Confirmed");
        changes.Should().OnlyContain(h => !string.IsNullOrEmpty(h.Reason));
        changes.Select(h => h.Actor).Should().Equal("Tourist", "System", "OperationsManager", "Tourist", "OperationsManager");
    }

    [Fact]
    public async Task Tourist_declines_with_a_reason_and_the_trip_goes_back_to_review()
    {
        await using var factory = new TestWebApplicationFactory();
        var (trip, outcome) = await factory.RunToProposalAsync();
        await factory.SendToClientAsync(outcome.QuotationId!.Value);
        var tourist = await factory.CreateClientAsAsync(WorkflowFlow.Tourist);

        var noReason = await tourist.PostAsJsonAsync($"/api/quotations/{outcome.QuotationId}/decline", new DeclineQuotationRequest(""));
        var response = await tourist.PostAsJsonAsync($"/api/quotations/{outcome.QuotationId}/decline",
            new DeclineQuotationRequest("Too expensive for us"));

        noReason.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync<QuotationDecisionResponse>(TestJson.Options))!.TripStatus.Should().Be("PendingReview");
        factory.State<FakeQuotationsState>().Quotations.Single().Status.Should().Be("Declined");
        factory.State<FakeQuotationsState>().Decisions.Should().Contain(d => d.Decision == QuotationDecision.Declined && d.Comment == "Too expensive for us");
        var notification = await factory.QueryDbAsync(db => db.Notifications.FirstAsync(n => n.Type == "ClientDeclined"));
        notification.Body.Should().Contain("Too expensive for us");
        // A declined version cannot be confirmed or accepted again.
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);
        (await manager.PostAsync($"/api/trip-requests/{trip.Id}/confirm", null)).StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await tourist.PostAsync($"/api/quotations/{outcome.QuotationId}/accept", null)).StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Confirm_with_a_conflicting_hold_returns_409_rolls_back_and_the_trip_stays_ClientAccepted()
    {
        await using var factory = new TestWebApplicationFactory();
        var (trip, outcome) = await factory.RunToClientAcceptedAsync();
        var resources = factory.State<FakeResourcesState>();
        // Someone else booked the van for the same dates after the proposal was validated.
        // The guide hold is staged first, so this also proves an already-staged hold is thrown away.
        resources.Holds.Add(new ResourceHoldRequest(ResourceType.Vehicle, FakeResourcesState.VanSixSeats, Guid.NewGuid(),
            trip.StartDate.AddDays(1), trip.StartDate.AddDays(2), 1));
        var before = await CountRowsAsync(factory);
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);

        var response = await manager.PostAsync($"/api/trip-requests/{trip.Id}/confirm", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        (await CountRowsAsync(factory)).Should().Be(before);
        resources.Holds.Should().ContainSingle(); // only the other trip's hold
        factory.State<FakeQuotationsState>().Decisions.Should().NotContain(d => d.Decision == QuotationDecision.Confirmed);
        (await factory.QueryDbAsync(db => db.TripRequests.SingleAsync(t => t.Id == trip.Id))).Status
            .Should().Be(TripRequestStatus.ClientAccepted);

        // The manager reopens the review to change the trip.
        var reopened = await manager.PostAsJsonAsync($"/api/trip-requests/{trip.Id}/reopen-review",
            new CancelTripRequest("The van is no longer free"));
        reopened.StatusCode.Should().Be(HttpStatusCode.OK);
        (await reopened.Content.ReadFromJsonAsync<QuotationDecisionResponse>(TestJson.Options))!.TripStatus.Should().Be("PendingReview");
    }

    [Fact]
    public async Task Reject_cancels_the_trip()
    {
        await using var factory = new TestWebApplicationFactory();
        var (trip, outcome) = await factory.RunToProposalAsync();
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);

        var response = await manager.PostAsJsonAsync($"/api/quotations/{outcome.QuotationId}/reject",
            new QuotationDecisionRequest("Dates not possible"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        factory.State<FakeQuotationsState>().Quotations.Single().Status.Should().Be("Rejected");
        factory.State<FakeResourcesState>().Holds.Should().BeEmpty();
        var (tripStatus, workflowStatus) = await factory.QueryDbAsync(async db => (
            (await db.TripRequests.SingleAsync(t => t.Id == trip.Id)).Status,
            (await db.AgentWorkflows.SingleAsync(w => w.Id == outcome.WorkflowId)).Status));
        tripStatus.Should().Be(TripRequestStatus.Cancelled);
        workflowStatus.Should().Be(AgentWorkflowStatus.Rejected);
    }

    [Fact]
    public async Task Request_revision_needs_a_comment_and_the_next_proposal_is_version_2_in_review()
    {
        await using var factory = new TestWebApplicationFactory();
        var (trip, outcome) = await factory.RunToProposalAsync(budgetUsd: 400); // over budget -> warning in review
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);

        var empty = await manager.PostAsJsonAsync($"/api/quotations/{outcome.QuotationId}/request-revision",
            new RequestRevisionRequest(""));
        var response = await manager.PostAsJsonAsync($"/api/quotations/{outcome.QuotationId}/request-revision",
            new RequestRevisionRequest("Use a cheaper hotel tier"));

        empty.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        factory.State<FakeAgentState>().Calls.Should().ContainSingle(c =>
            c.Kind == "replan" && c.WorkflowId == outcome.WorkflowId && c.Comment == "Use a cheaper hotel tier");
        (await factory.QueryDbAsync(db => db.TripRequests.SingleAsync(t => t.Id == trip.Id))).Status
            .Should().Be(TripRequestStatus.RevisionRequested);

        // The Planner's new proposal brings the trip back to review as version 2.
        var replan = TestProposals.Golden(trip.StartDate, await factory.SeededAttractionsAsync());
        (await factory.PostProposalAsync(outcome.WorkflowId, replan)).EnsureSuccessStatusCode();
        factory.State<FakeQuotationsState>().Quotations.Select(q => (q.Version, q.Status))
            .Should().Equal((1, "RevisionRequested"), (2, "Pending"));
        (await factory.QueryDbAsync(db => db.TripRequests.SingleAsync(t => t.Id == trip.Id))).Status
            .Should().Be(TripRequestStatus.PendingReview);
    }

    [Fact]
    public async Task Failed_replan_call_ends_the_workflow_and_the_trip_safely()
    {
        await using var factory = new TestWebApplicationFactory();
        var (trip, outcome) = await factory.RunToProposalAsync();
        factory.State<FakeAgentState>().Fail = true;
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);

        var response = await manager.PostAsJsonAsync($"/api/quotations/{outcome.QuotationId}/request-revision",
            new RequestRevisionRequest("Add Galle"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var workflow = await factory.QueryDbAsync(db => db.AgentWorkflows.SingleAsync(w => w.Id == outcome.WorkflowId));
        workflow.Status.Should().Be(AgentWorkflowStatus.FailedSafely);
        workflow.ErrorSummary.Should().Contain("connection refused");
        (await factory.QueryDbAsync(db => db.TripRequests.SingleAsync(t => t.Id == trip.Id))).Status
            .Should().Be(TripRequestStatus.FailedSafely);
    }

    [Theory]
    [InlineData("tourist1@tripcraft.test")]
    [InlineData("admin1@tripcraft.test")]
    public async Task Only_an_operations_manager_can_send_or_confirm(string email)
    {
        await using var factory = new TestWebApplicationFactory();
        var (trip, outcome) = await factory.RunToProposalAsync();
        var client = await factory.CreateClientAsAsync(email);

        (await client.PostAsync($"/api/quotations/{outcome.QuotationId}/approve", null)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await client.PostAsync($"/api/trip-requests/{trip.Id}/confirm", null)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Only_the_owner_tourist_can_accept()
    {
        await using var factory = new TestWebApplicationFactory();
        var (_, outcome) = await factory.RunToProposalAsync();
        await factory.SendToClientAsync(outcome.QuotationId!.Value);
        var other = await factory.CreateClientAsAsync(WorkflowFlow.OtherTourist);
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);

        (await other.PostAsync($"/api/quotations/{outcome.QuotationId}/accept", null)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await manager.PostAsync($"/api/quotations/{outcome.QuotationId}/accept", null)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Sending_twice_returns_409()
    {
        await using var factory = new TestWebApplicationFactory();
        var (_, outcome) = await factory.RunToProposalAsync();
        await factory.SendToClientAsync(outcome.QuotationId!.Value);
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);

        var second = await manager.PostAsync($"/api/quotations/{outcome.QuotationId}/approve", null);

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    private static Task<(int Audit, int Workflows, int Vouchers, int Trips)> CountRowsAsync(TestWebApplicationFactory factory) =>
        factory.QueryDbAsync(async db => (
            await db.AuditLogs.CountAsync(), await db.AgentWorkflows.CountAsync(),
            await db.Vouchers.CountAsync(), await db.TripRequests.CountAsync()));
}
