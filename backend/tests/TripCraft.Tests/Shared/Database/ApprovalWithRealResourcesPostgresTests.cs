using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Resources;
using TripCraft.Application.Trips;
using TripCraft.Tests.Common;
using TripCraft.Tests.Workflows;

namespace TripCraft.Tests.Shared.Database;

/// <summary>
/// The API on PostgreSQL with the real Resource Management (seeded guides, van, hotels) and Quotation components.
/// </summary>
public class RealResourcesPostgresFactory(string connectionString) : PostgresWebApplicationFactory(connectionString)
{
    protected override bool UseRealResourceManagement => true;
    protected override bool UseRealQuotations => true;
}

/// <summary>
/// Spec checklist "conflicting hold on approve → 409 and no partial rows" (v1.1: on Confirm), with B's real hold service:
/// two trips for the same dates were both accepted by the client; the first Confirm holds the guide,
/// the second is refused and leaves nothing behind.
/// </summary>
[Collection(PostgresCollection.Name)]
public class ApprovalWithRealResourcesPostgresTests(PostgresFixture postgres)
{
    [Fact]
    public async Task First_confirm_writes_resource_holds_second_for_the_same_guide_is_409_with_no_partial_rows()
    {
        await using var factory = new RealResourcesPostgresFactory(await postgres.CreateMigratedDatabaseAsync());
        var (tripA, outcomeA) = await factory.RunToClientAcceptedAsync();
        var (tripB, outcomeB) = await factory.RunToClientAcceptedAsync();
        outcomeA.Status.Should().Be("Approved"); // both sent to the client automatically
        outcomeB.Status.Should().Be("Approved");
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);

        (await manager.PostAsync($"/api/trip-requests/{tripA.Id}/confirm", null)).StatusCode.Should().Be(HttpStatusCode.OK);
        var second = await manager.PostAsync($"/api/trip-requests/{tripB.Id}/confirm", null);

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
        statusB.Should().Be(TripRequestStatus.ClientAccepted);

        // Component C's rows: A sent, accepted and confirmed; B sent and accepted but not confirmed.
        var (statusA, decisionsA, quotationB, decisionsB) = await factory.QueryDbAsync(async db => (
            (await db.Quotations.SingleAsync(q => q.Id == outcomeA.QuotationId)).Status,
            await db.ApprovalDecisions.CountAsync(d => d.QuotationId == outcomeA.QuotationId),
            (await db.Quotations.SingleAsync(q => q.Id == outcomeB.QuotationId)).Status,
            await db.ApprovalDecisions.CountAsync(d => d.QuotationId == outcomeB.QuotationId)));
        statusA.Should().Be(Application.Quotations.QuotationStatus.Approved);
        decisionsA.Should().Be(2); // Accepted, Confirmed (the send was automatic)
        quotationB.Should().Be(Application.Quotations.QuotationStatus.Approved);
        decisionsB.Should().Be(1); // Accepted
    }
}
