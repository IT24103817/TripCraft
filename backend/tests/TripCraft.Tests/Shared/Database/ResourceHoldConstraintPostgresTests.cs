using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using TripCraft.Application.Resources;
using TripCraft.Application.Workflows.Ports;
using TripCraft.Infrastructure.Persistence;

namespace TripCraft.Tests.Shared.Database;

/// <summary>
/// PLAN.md section 11 "hold overlap rejected": PostgreSQL itself refuses a second Held hold of the same guide or
/// vehicle for overlapping dates (exclusion constraint), even if the service check were skipped or raced.
/// </summary>
[Collection(PostgresCollection.Name)]
public class ResourceHoldConstraintPostgresTests(PostgresFixture postgres) : IAsyncLifetime
{
    private static readonly DateOnly Start = new(2026, 10, 10);
    private string _connectionString = string.Empty;
    private readonly Guid _guideId = Guid.NewGuid();

    public async Task InitializeAsync()
    {
        _connectionString = await postgres.CreateMigratedDatabaseAsync();
        await using var db = Db();
        db.Guides.Add(new Guide { Id = _guideId, Name = "Nimal", Phone = "+94 77 1", DayRateLkr = 6000, MaxPax = 10 });
        await db.SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private AppDbContext Db() => PostgresFixture.CreateContext(_connectionString);

    private ResourceHold GuideHold(int fromOffset, int toOffset, HoldStatus status = HoldStatus.Held) => new()
    {
        ResourceType = ResourceType.Guide, ResourceId = _guideId, FromDate = Start.AddDays(fromOffset),
        ToDate = Start.AddDays(toOffset), Status = status
    };

    private static async Task<string> SqlStateOf(Func<Task> save) =>
        (await save.Should().ThrowAsync<DbUpdateException>()).Which.InnerException.Should().BeOfType<PostgresException>()
        .Which.SqlState;

    [Fact]
    public async Task Overlapping_held_guide_holds_violate_the_exclusion_constraint()
    {
        await using (var db = Db())
        {
            db.ResourceHolds.Add(GuideHold(0, 4));
            await db.SaveChangesAsync();
        }

        await using var second = Db();
        second.ResourceHolds.Add(GuideHold(4, 6)); // shares the last day
        (await SqlStateOf(() => second.SaveChangesAsync())).Should().Be(PostgresErrorCodes.ExclusionViolation);
    }

    [Fact]
    public async Task Back_to_back_or_released_holds_are_allowed()
    {
        await using var db = Db();
        db.ResourceHolds.AddRange(GuideHold(0, 4), GuideHold(5, 6), GuideHold(0, 6, HoldStatus.Released));

        await db.Invoking(d => d.SaveChangesAsync()).Should().NotThrowAsync();
    }

    [Fact]
    public async Task Room_holds_are_not_exclusive_but_dates_and_quantity_are_checked()
    {
        await using var db = Db();
        var roomType = Guid.NewGuid();
        db.ResourceHolds.AddRange(
            new ResourceHold { ResourceType = ResourceType.Room, ResourceId = roomType, FromDate = Start, ToDate = Start, Quantity = 2 },
            new ResourceHold { ResourceType = ResourceType.Room, ResourceId = roomType, FromDate = Start, ToDate = Start, Quantity = 1 });
        await db.SaveChangesAsync(); // the per-night room limit is enforced by ResourceHoldService

        await using var bad = Db();
        bad.ResourceHolds.Add(new ResourceHold { ResourceType = ResourceType.Room, ResourceId = roomType, FromDate = Start, ToDate = Start.AddDays(-1) });
        (await SqlStateOf(() => bad.SaveChangesAsync())).Should().Be(PostgresErrorCodes.CheckViolation);
    }

    [Fact]
    public async Task Vehicle_registration_is_unique()
    {
        await using (var db = Db())
        {
            db.Vehicles.Add(new Vehicle { RegistrationNo = "CAB-1234", Type = "Van", Seats = 6, RatePerKmLkr = 120 });
            await db.SaveChangesAsync();
        }
        await using var second = Db();
        second.Vehicles.Add(new Vehicle { RegistrationNo = "CAB-1234", Type = "Car", Seats = 3, RatePerKmLkr = 100 });
        (await SqlStateOf(() => second.SaveChangesAsync())).Should().Be(PostgresErrorCodes.UniqueViolation);
    }
}
