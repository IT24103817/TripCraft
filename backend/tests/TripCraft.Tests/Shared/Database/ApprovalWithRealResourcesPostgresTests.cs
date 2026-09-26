using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Resources;
using TripCraft.Application.Trips;
using TripCraft.Tests.Common;
using TripCraft.Tests.Workflows;

namespace TripCraft.Tests.Shared.Database;

/// <summary>The API on PostgreSQL with Resource Management's real catalog and hold service (seeded guides, van, hotels).</summary>
public class RealResourcesPostgresFactory(string connectionString) : PostgresWebApplicationFactory(connectionString)
{
    protected override bool UseRealResourceManagement => true;
}

/// <summary>
/// Spec checklist "conflicting hold on approve → 409 and no partial rows", with B's real hold service:
/// two trips for the same dates are both waiting for approval; the first approval holds the guide,
/// the second is refused and leaves nothing behind.
/// </summary>
[Collection(PostgresCollection.Name)]
public class ApprovalWithRealResourcesPostgresTests(PostgresFixture postgres)
{
    [Fact]
    public async Task First_approval_writes_resource_holds_second_for_the_same_guide_is_409_with_no_partial_rows()
    {
        await using var factory = new RealResourcesPostgresFactory(await postgres.CreateMigratedDatabaseAsync());
        var (tripA, outcomeA) = await factory.RunToProposalAsync();
        var (tripB, outcomeB) = await factory.RunToProposalAsync();
        outcomeA.Status.Should().Be("PendingApproval");
        outcomeB.Status.Should().Be("PendingApproval");
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);

        (await manager.PostAsync($"/api/quotations/{outcomeA.QuotationId}/approve", null)).StatusCode.Should().Be(HttpStatusCode.OK);
        var second = await manager.PostAsync($"/api/quotations/{outcomeB.QuotationId}/approve", null);

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await second.Content.ReadAsStringAsync()).Should().Contain("Guide is already held");
        var (holdsA, holdsB, itinerariesB, statusB) = await factory.QueryDbAsync(async db => (
            await db.ResourceHolds.CountAsync(h => h.TripRequestId == tripA.Id && h.Status == HoldStatus.Held),
            await db.ResourceHolds.CountAsync(h => h.TripRequestId == tripB.Id),
            await db.Itineraries.CountAsync(i => i.TripRequestId == tripB.Id),
            (await db.TripRequests.SingleAsync(t => t.Id == tripB.Id)).Status));
        holdsA.Should().Be(6, "guide + van + 4 room-nights");
        holdsB.Should().Be(0);
        itinerariesB.Should().Be(0);
        statusB.Should().Be(TripRequestStatus.PendingApproval);
    }
}
