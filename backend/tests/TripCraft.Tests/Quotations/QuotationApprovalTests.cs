using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Quotations;
using TripCraft.Application.Trips;
using TripCraft.Application.Workflows;
using TripCraft.Application.Workflows.Ports;
using TripCraft.Tests.Common;
using TripCraft.Tests.Workflows;
using TripCraft.Tests.Workflows.Fakes;

namespace TripCraft.Tests.Quotations;

/// <summary>
/// The approval gate (PLAN.md section 6, step 9–10). Each test gets its own app, because approvals
/// create holds that would overlap with the next test's trip dates.
/// </summary>
public class QuotationApprovalTests
{
    [Fact]
    public async Task Approve_creates_holds_confirms_trip_completes_workflow_and_audits()
    {
        await using var factory = new TestWebApplicationFactory();
        var (trip, outcome) = await factory.RunToProposalAsync();
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);

        var response = await manager.PostAsJsonAsync($"/api/quotations/{outcome.QuotationId}/approve",
            new QuotationDecisionRequest("Looks good"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var decision = (await response.Content.ReadFromJsonAsync<QuotationDecisionResponse>(TestJson.Options))!;
        decision.TripStatus.Should().Be("Confirmed");
        decision.WorkflowStatus.Should().Be("Completed");
        decision.HoldsCreated.Should().Be(6); // guide + vehicle + 4 nights x 1 room type (2 rooms each)

        var holds = factory.State<FakeResourcesState>().Holds;
        holds.Should().HaveCount(6).And.OnlyContain(h => h.TripRequestId == trip.Id);
        holds.Should().Contain(h => h.Type == ResourceType.Guide && h.From == trip.StartDate && h.To == trip.EndDate);
        holds.Where(h => h.Type == ResourceType.Room).Should().OnlyContain(h => h.Quantity == 2);

        var quotations = factory.State<FakeQuotationsState>();
        quotations.Quotations.Single(q => q.Id == outcome.QuotationId).Status.Should().Be("Approved");
        quotations.Decisions.Should().ContainSingle(d => d.Decision == QuotationDecision.Approved && d.Comment == "Looks good");

        var (tripStatus, workflow, audit) = await factory.QueryDbAsync(async db => (
            (await db.TripRequests.SingleAsync(t => t.Id == trip.Id)).Status,
            await db.AgentWorkflows.SingleAsync(w => w.Id == outcome.WorkflowId),
            await db.AuditLogs.SingleOrDefaultAsync(a => a.Action == "QuotationApproved")));
        tripStatus.Should().Be(TripRequestStatus.Confirmed);
        workflow.Status.Should().Be(AgentWorkflowStatus.Completed);
        workflow.FinishedAt.Should().NotBeNull();
        workflow.FinalOutcome.Should().Contain("\"decision\":\"Approved\"");
        audit.Should().NotBeNull();

        // Step 11: the tourist now sees the saved itinerary (5 days) instead of the proposal.
        var tourist = await factory.CreateClientAsAsync("tourist1@tripcraft.test");
        var itinerary = await tourist.GetFromJsonAsync<Application.Trips.Dtos.ItineraryDto>(
            $"/api/trip-requests/{trip.Id}/itinerary", TestJson.Options);
        itinerary!.Days.Should().HaveCount(5);
        itinerary.Days[0].Stops.Should().ContainSingle();
    }

    [Fact]
    public async Task Conflicting_hold_returns_409_and_rolls_everything_back()
    {
        await using var factory = new TestWebApplicationFactory();
        var (trip, outcome) = await factory.RunToProposalAsync();
        var resources = factory.State<FakeResourcesState>();
        var quotations = factory.State<FakeQuotationsState>();
        // Someone else booked the van for the same dates after the proposal was validated.
        // The guide hold is staged first, so this also proves an already-staged hold is thrown away.
        resources.Holds.Add(new ResourceHoldRequest(ResourceType.Vehicle, FakeResourcesState.VanSixSeats, Guid.NewGuid(),
            trip.StartDate.AddDays(1), trip.StartDate.AddDays(2), 1));
        var before = await CountRowsAsync(factory);
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);

        var response = await manager.PostAsync($"/api/quotations/{outcome.QuotationId}/approve", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        (await CountRowsAsync(factory)).Should().Be(before);
        resources.Holds.Should().ContainSingle(); // only the other trip's hold
        quotations.Decisions.Should().BeEmpty();
        quotations.Quotations.Single(q => q.Id == outcome.QuotationId).Status.Should().Be("Pending");
        var (tripStatus, workflowStatus) = await factory.QueryDbAsync(async db => (
            (await db.TripRequests.SingleAsync(t => t.Id == trip.Id)).Status,
            (await db.AgentWorkflows.SingleAsync(w => w.Id == outcome.WorkflowId)).Status));
        tripStatus.Should().Be(TripRequestStatus.PendingApproval);
        workflowStatus.Should().Be(AgentWorkflowStatus.PendingApproval);
    }

    [Fact]
    public async Task Reject_sets_rejected_on_quotation_trip_and_workflow()
    {
        await using var factory = new TestWebApplicationFactory();
        var (trip, outcome) = await factory.RunToProposalAsync();
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);

        var response = await manager.PostAsJsonAsync($"/api/quotations/{outcome.QuotationId}/reject",
            new QuotationDecisionRequest("Dates not possible"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        factory.State<FakeQuotationsState>().Quotations.Single().Status.Should().Be("Rejected");
        factory.State<FakeQuotationsState>().Decisions.Should().ContainSingle(d => d.Decision == QuotationDecision.Rejected);
        factory.State<FakeResourcesState>().Holds.Should().BeEmpty();
        var (tripStatus, workflowStatus, audited) = await factory.QueryDbAsync(async db => (
            (await db.TripRequests.SingleAsync(t => t.Id == trip.Id)).Status,
            (await db.AgentWorkflows.SingleAsync(w => w.Id == outcome.WorkflowId)).Status,
            await db.AuditLogs.AnyAsync(a => a.Action == "QuotationRejected")));
        tripStatus.Should().Be(TripRequestStatus.Rejected);
        workflowStatus.Should().Be(AgentWorkflowStatus.Rejected);
        audited.Should().BeTrue();
    }

    [Fact]
    public async Task Request_revision_needs_a_comment_and_sends_it_to_the_planner()
    {
        await using var factory = new TestWebApplicationFactory();
        var (trip, outcome) = await factory.RunToProposalAsync(budgetUsd: 400); // over budget -> RevisionRequested
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);

        var empty = await manager.PostAsJsonAsync($"/api/quotations/{outcome.QuotationId}/request-revision",
            new RequestRevisionRequest(""));
        var response = await manager.PostAsJsonAsync($"/api/quotations/{outcome.QuotationId}/request-revision",
            new RequestRevisionRequest("Use a cheaper hotel tier"));

        empty.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        factory.State<FakeAgentState>().Calls.Should().ContainSingle(c =>
            c.Kind == "replan" && c.WorkflowId == outcome.WorkflowId && c.Comment == "Use a cheaper hotel tier");
        factory.State<FakeQuotationsState>().Quotations.Single().Status.Should().Be("RevisionRequested");
        (await factory.QueryDbAsync(db => db.TripRequests.SingleAsync(t => t.Id == trip.Id))).Status
            .Should().Be(TripRequestStatus.RevisionRequested);
    }

    [Fact]
    public async Task Failed_replan_call_ends_the_workflow_safely()
    {
        await using var factory = new TestWebApplicationFactory();
        var (_, outcome) = await factory.RunToProposalAsync();
        factory.State<FakeAgentState>().Fail = true;
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);

        var response = await manager.PostAsJsonAsync($"/api/quotations/{outcome.QuotationId}/request-revision",
            new RequestRevisionRequest("Add Galle"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var workflow = await factory.QueryDbAsync(db => db.AgentWorkflows.SingleAsync(w => w.Id == outcome.WorkflowId));
        workflow.Status.Should().Be(AgentWorkflowStatus.FailedSafely);
        workflow.ErrorSummary.Should().Contain("connection refused");
    }

    [Theory]
    [InlineData("tourist1@tripcraft.test")]
    [InlineData("admin1@tripcraft.test")]
    public async Task Only_an_operations_manager_can_approve(string email)
    {
        await using var factory = new TestWebApplicationFactory();
        var (_, outcome) = await factory.RunToProposalAsync();
        var client = await factory.CreateClientAsAsync(email);

        var response = await client.PostAsync($"/api/quotations/{outcome.QuotationId}/approve", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        factory.State<FakeResourcesState>().Holds.Should().BeEmpty();
    }

    [Fact]
    public async Task Approving_twice_returns_409()
    {
        await using var factory = new TestWebApplicationFactory();
        var (_, outcome) = await factory.RunToProposalAsync();
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);
        (await manager.PostAsync($"/api/quotations/{outcome.QuotationId}/approve", null)).EnsureSuccessStatusCode();

        var second = await manager.PostAsync($"/api/quotations/{outcome.QuotationId}/approve", null);

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
        factory.State<FakeResourcesState>().Holds.Should().HaveCount(6);
    }

    private static Task<(int Audit, int Workflows, int Steps, int Trips)> CountRowsAsync(TestWebApplicationFactory factory) =>
        factory.QueryDbAsync(async db => (
            await db.AuditLogs.CountAsync(), await db.AgentWorkflows.CountAsync(),
            await db.AgentSteps.CountAsync(), await db.TripRequests.CountAsync()));
}
