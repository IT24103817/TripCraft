using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Trips;
using TripCraft.Application.Workflows;
using TripCraft.Application.Workflows.Ports;
using TripCraft.Tests.Common;
using TripCraft.Tests.Workflows;
using TripCraft.Tests.Workflows.Fakes;

namespace TripCraft.Tests.Shared.Database;

/// <summary>
/// The approval transaction on a real PostgreSQL transaction (InMemory cannot roll back).
/// Holds are Resource Management's (Student B) table; until it is merged they live in the fake hold
/// service, which only commits when the DbContext commits.
/// </summary>
[Collection(PostgresCollection.Name)]
public class ApprovalTransactionPostgresTests(PostgresFixture postgres)
{
    private async Task<PostgresWebApplicationFactory> AppAsync() =>
        new(await postgres.CreateMigratedDatabaseAsync());

    [Fact]
    public async Task Conflict_during_confirm_rolls_back_and_leaves_zero_holds_for_the_trip()
    {
        await using var factory = await AppAsync();
        var (trip, outcome) = await factory.RunToClientAcceptedAsync();
        var resources = factory.State<FakeResourcesState>();
        // The van gets booked by another trip between validation and confirmation.
        resources.Holds.Add(new ResourceHoldRequest(ResourceType.Vehicle, FakeResourcesState.VanSixSeats, Guid.NewGuid(),
            trip.StartDate, trip.StartDate, 1));
        var auditBefore = await factory.QueryDbAsync(db => db.AuditLogs.CountAsync());
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);

        var response = await manager.PostAsync($"/api/trip-requests/{trip.Id}/confirm", null);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        resources.Holds.Where(h => h.TripRequestId == trip.Id).Should().BeEmpty();
        factory.State<FakeQuotationsState>().Decisions.Should().NotContain(d => d.Decision == QuotationDecision.Confirmed);
        var (tripStatus, workflowStatus, auditAfter) = await factory.QueryDbAsync(async db => (
            (await db.TripRequests.SingleAsync(t => t.Id == trip.Id)).Status,
            (await db.AgentWorkflows.SingleAsync(w => w.Id == outcome.WorkflowId)).Status,
            await db.AuditLogs.CountAsync()));
        tripStatus.Should().Be(TripRequestStatus.ClientAccepted);
        workflowStatus.Should().Be(AgentWorkflowStatus.Approved);
        auditAfter.Should().Be(auditBefore);
        (await factory.QueryDbAsync(db => db.Itineraries.CountAsync(i => i.TripRequestId == trip.Id))).Should().Be(0);
        (await factory.QueryDbAsync(db => db.Vouchers.CountAsync(v => v.TripRequestId == trip.Id))).Should().Be(0);
    }

    [Fact]
    public async Task Successful_confirm_commits_everything_in_postgres()
    {
        await using var factory = await AppAsync();
        var (trip, outcome) = await factory.RunToClientAcceptedAsync();
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);

        var response = await manager.PostAsync($"/api/trip-requests/{trip.Id}/confirm", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        factory.State<FakeResourcesState>().Holds.Where(h => h.TripRequestId == trip.Id).Should().HaveCount(6);
        var (tripStatus, workflow, audited) = await factory.QueryDbAsync(async db => (
            (await db.TripRequests.SingleAsync(t => t.Id == trip.Id)).Status,
            await db.AgentWorkflows.SingleAsync(w => w.Id == outcome.WorkflowId),
            await db.AuditLogs.AnyAsync(a => a.Action == "TripConfirmed")));
        tripStatus.Should().Be(TripRequestStatus.Confirmed);
        workflow.Status.Should().Be(AgentWorkflowStatus.Completed);
        workflow.FinalOutcome.Should().Contain("\"decision\": \"Confirmed\"").And.Contain("\"holds\"");
        audited.Should().BeTrue();
        var days = await factory.QueryDbAsync(db => db.ItineraryDays
            .Where(d => db.Itineraries.Any(i => i.Id == d.ItineraryId && i.TripRequestId == trip.Id)).CountAsync());
        days.Should().Be(5); // the saved itinerary was committed in the same transaction
        (await factory.QueryDbAsync(db => db.Vouchers.CountAsync(v => v.TripRequestId == trip.Id))).Should().Be(5);
    }
}
