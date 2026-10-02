using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Trips;
using TripCraft.Application.Workflows;
using TripCraft.Application.Workflows.Dtos;
using TripCraft.Tests.Common;
using TripCraft.Tests.Workflows.Fakes;

namespace TripCraft.Tests.Workflows;

/// <summary>POST /api/internal/workflows/{id}/proposal runs ProposalValidator and sets the next status.</summary>
public class ProposalEndpointTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    [Fact]
    public async Task Golden_proposal_is_sent_to_the_client_with_a_quotation_and_the_tourist_is_notified()
    {
        var (trip, outcome) = await factory.RunToProposalAsync();

        outcome.Status.Should().Be("Approved"); // the agent workflow: quotation sent
        outcome.Validation.IsValid.Should().BeTrue();
        // Approval gate (PLAN.md section 5): a valid proposal holds nothing; holds come only with Confirm.
        factory.State<FakeResourcesState>().Holds.Should().NotContain(h => h.TripRequestId == trip.Id);
        var quotation = factory.State<FakeQuotationsState>().Quotations.Single(q => q.Id == outcome.QuotationId);
        quotation.Draft.TotalLkr.Should().Be(TestProposals.GoldenTotalLkr);
        quotation.Version.Should().Be(1);
        quotation.Draft.ProposalSnapshot.Should().Contain("\"days\"");

        var (workflow, tripStatus, audited) = await factory.QueryDbAsync(async db => (
            await db.AgentWorkflows.SingleAsync(w => w.Id == outcome.WorkflowId),
            (await db.TripRequests.SingleAsync(t => t.Id == trip.Id)).Status,
            await db.AuditLogs.AnyAsync(a => a.EntityId == outcome.WorkflowId && a.Action == "AgentProposalReceived")));
        tripStatus.Should().Be(TripRequestStatus.QuotationSent);
        (await factory.QueryDbAsync(db => db.Notifications.AnyAsync(n => n.Type == "QuotationSent" && n.TripRequestId == trip.Id)))
            .Should().BeTrue();
        workflow.ValidationResult.Should().Contain("\"isValid\":true");
        workflow.Plan.Should().Contain("Kandy");
        workflow.FinalOutcome.Should().Contain("\"quotationId\"");
        audited.Should().BeTrue();
    }

    [Fact]
    public async Task Over_budget_proposal_is_sent_anyway_as_the_best_available_price()
    {
        var (trip, outcome) = await factory.RunToProposalAsync(budgetUsd: 400);

        outcome.Status.Should().Be("Approved");
        outcome.Validation.Violations.Should().ContainSingle(v => v.Code == "OVER_BUDGET" && v.Severity == ViolationSeverity.Soft);
        outcome.QuotationId.Should().NotBeNull();
        (await factory.QueryDbAsync(db => db.TripRequests.SingleAsync(t => t.Id == trip.Id))).Status
            .Should().Be(TripRequestStatus.QuotationSent);
        var quotation = factory.State<FakeQuotationsState>().Quotations.Single(q => q.Id == outcome.QuotationId);
        quotation.Status.Should().Be("Approved");
        quotation.Draft.OverBudgetUsd.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Hard_violation_fails_safely_without_a_quotation_and_the_trip_needs_the_operator()
    {
        var (trip, workflowId) = await factory.StartPlanningAsync();
        var a = await factory.SeededAttractionsAsync();
        var proposal = TestProposals.Golden(trip.StartDate, a);
        proposal.Days![3] = TestProposals.Day(4, trip.StartDate.AddDays(3), "Ella", 0,
            TestProposals.Stop(a.AdamsPeak, 0), TestProposals.Stop(a.NineArches, 0),
            TestProposals.Stop(a.AdamsPeak, 0), TestProposals.Stop(a.NineArches, 0));
        var quotationsBefore = factory.State<FakeQuotationsState>().Quotations.Count;

        var response = await factory.PostProposalAsync(workflowId, proposal);

        var outcome = (await response.Content.ReadFromJsonAsync<ProposalOutcomeResponse>(TestJson.Options))!;
        outcome.Status.Should().Be("FailedSafely");
        outcome.QuotationId.Should().BeNull();
        factory.State<FakeQuotationsState>().Quotations.Count.Should().Be(quotationsBefore);
        var (workflow, tripStatus) = await factory.QueryDbAsync(async db => (
            await db.AgentWorkflows.SingleAsync(w => w.Id == workflowId),
            (await db.TripRequests.SingleAsync(t => t.Id == trip.Id)).Status));
        workflow.ErrorSummary.Should().Contain("DAY_STOPS");
        workflow.FinishedAt.Should().NotBeNull();
        tripStatus.Should().Be(TripRequestStatus.NeedsOperator); // never sent to the client
    }

    [Fact]
    public async Task Agent_reported_safe_failure_is_recorded()
    {
        var (_, workflowId) = await factory.StartPlanningAsync();
        var failed = new AgentProposalRequest(null, [], null, null, [], "FailedSafely", 0,
            "itinerary: GET /api/internal/distance returned 503");

        var outcome = (await (await factory.PostProposalAsync(workflowId, failed))
            .Content.ReadFromJsonAsync<ProposalOutcomeResponse>(TestJson.Options))!;

        outcome.Status.Should().Be("FailedSafely");
        (await factory.QueryDbAsync(db => db.AgentWorkflows.SingleAsync(w => w.Id == workflowId))).ErrorSummary
            .Should().Contain("distance returned 503");
    }

    [Fact]
    public async Task Every_trip_status_change_from_a_proposal_is_audited()
    {
        // Golden: Planning -> QuotationSent (auto-sent).
        var (golden, _) = await factory.RunToProposalAsync();
        // Agent failure: Planning -> NeedsOperator (the operator retries, sends by hand or cancels).
        var (failedTrip, workflowId) = await factory.StartPlanningAsync();
        await factory.PostProposalAsync(workflowId, new AgentProposalRequest(null, [], null, null, [], "FailedSafely", 0,
            "resources: GET /api/internal/availability/guides returned 503"));

        var changes = await factory.QueryDbAsync(db => db.AuditLogs
            .Where(a => (a.EntityId == golden.Id || a.EntityId == failedTrip.Id) && a.Action == "TripRequestStatusChanged")
            .Select(a => new { a.EntityId, a.After })
            .ToListAsync());

        changes.Should().Contain(c => c.EntityId == golden.Id && c.After!.Contains("QuotationSent"));
        changes.Should().Contain(c => c.EntityId == failedTrip.Id && c.After!.Contains("NeedsOperator") && c.After.Contains("resources: GET"));
    }

    [Fact]
    public async Task A_decided_workflow_no_longer_accepts_proposals()
    {
        var (trip, outcome) = await factory.RunToProposalAsync();
        var a = await factory.SeededAttractionsAsync();
        await factory.QueryDbAsync(async db =>
        {
            (await db.AgentWorkflows.SingleAsync(w => w.Id == outcome.WorkflowId)).Status = AgentWorkflowStatus.Completed;
            return await db.SaveChangesAsync();
        });

        var response = await factory.PostProposalAsync(outcome.WorkflowId, TestProposals.Golden(trip.StartDate, a));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
