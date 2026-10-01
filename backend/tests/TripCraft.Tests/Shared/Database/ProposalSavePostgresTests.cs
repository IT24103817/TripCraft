using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Trips;
using TripCraft.Application.Trips.Dtos;
using TripCraft.Application.Workflows;
using TripCraft.Application.Workflows.Dtos;
using TripCraft.Tests.Common;
using TripCraft.Tests.Workflows;

namespace TripCraft.Tests.Shared.Database;

/// <summary>
/// A proposal must always end the workflow in PostgreSQL. Found in the live run: a one-city trip has no transfer
/// km, the agent sent a vehicle line with qty 0, ck_quotation_lines_amounts rejected it, the callback got a 500
/// and the workflow stayed Planning for ever.
/// </summary>
[Collection(PostgresCollection.Name)]
public class ProposalSavePostgresTests(PostgresFixture postgres)
{
    private static async Task<(TripRequestDto Trip, Guid WorkflowId, AgentProposalRequest Golden)> PlanningAsync(
        TestWebApplicationFactory factory)
    {
        var (trip, workflowId) = await factory.StartPlanningAsync();
        return (trip, workflowId, TestProposals.Golden(trip.StartDate, await factory.SeededAttractionsAsync()));
    }

    [Fact]
    public async Task A_zero_km_vehicle_line_is_not_stored_and_the_trip_waits_for_approval()
    {
        await using var factory = new RealResourcesPostgresFactory(await postgres.CreateMigratedDatabaseAsync());
        var (trip, workflowId, golden) = await PlanningAsync(factory);
        var proposal = golden with
        {
            Quotation = golden.Quotation! with
            {
                Lines = [.. golden.Quotation.Lines!, new ProposalQuotationLine("vehicle", "Vehicle", 0, 120, 0)]
            }
        };

        var response = await factory.PostProposalAsync(workflowId, proposal);

        response.EnsureSuccessStatusCode();
        (await response.Content.ReadFromJsonAsync<ProposalOutcomeResponse>(TestJson.Options))!.Status
            .Should().Be("PendingApproval");
        var lines = await factory.QueryDbAsync(db => db.QuotationLines
            .Where(l => db.Quotations.Any(q => q.Id == l.QuotationId && q.TripRequestId == trip.Id)).ToListAsync());
        lines.Should().NotBeEmpty().And.OnlyContain(l => l.Qty > 0);
    }

    [Fact]
    public async Task A_proposal_the_database_rejects_ends_failed_safely_instead_of_staying_planning()
    {
        await using var factory = new RealResourcesPostgresFactory(await postgres.CreateMigratedDatabaseAsync());
        var (trip, workflowId, golden) = await PlanningAsync(factory);
        var proposal = golden with
        {
            Quotation = golden.Quotation! with
            {
                Lines = [.. golden.Quotation.Lines!, new ProposalQuotationLine("entry", "Broken", 1, -5, -5)] // amount < 0
            }
        };

        var response = await factory.PostProposalAsync(workflowId, proposal);

        response.EnsureSuccessStatusCode();
        var outcome = (await response.Content.ReadFromJsonAsync<ProposalOutcomeResponse>(TestJson.Options))!;
        outcome.Status.Should().Be("FailedSafely");
        outcome.Validation.Violations.Should().ContainSingle(v => v.Code == "PROPOSAL_NOT_SAVED");
        var (workflow, tripStatus, quotations) = await factory.QueryDbAsync(async db => (
            await db.AgentWorkflows.SingleAsync(w => w.Id == workflowId),
            (await db.TripRequests.SingleAsync(t => t.Id == trip.Id)).Status,
            await db.Quotations.CountAsync(q => q.TripRequestId == trip.Id)));
        workflow.Status.Should().Be(AgentWorkflowStatus.FailedSafely);
        tripStatus.Should().Be(TripRequestStatus.FailedSafely); // the tourist can try again
        quotations.Should().Be(0);
    }
}
