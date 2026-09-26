using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using TripCraft.Application.Identity;
using TripCraft.Application.Quotations;
using TripCraft.Application.Trips;
using TripCraft.Infrastructure.Persistence;

namespace TripCraft.Tests.Shared.Database;

/// <summary>Component C's tables reject bad rows in PostgreSQL itself.</summary>
[Collection(PostgresCollection.Name)]
public class QuotationConstraintPostgresTests(PostgresFixture postgres) : IAsyncLifetime
{
    private string _connectionString = string.Empty;
    private Guid _tripId;

    public async Task InitializeAsync()
    {
        _connectionString = await postgres.CreateMigratedDatabaseAsync();
        await using var db = Db();
        var user = new User { Email = "q@tripcraft.test", FullName = "Q", PasswordHash = "hash", Role = UserRole.Tourist };
        var tourist = new Tourist { UserId = user.Id, Nationality = "UK", PassportNumberMasked = "****4567" };
        var trip = new TripRequest
        {
            TouristId = tourist.Id, Objective = "Kandy", StartDate = new DateOnly(2026, 10, 10),
            EndDate = new DateOnly(2026, 10, 12), Pax = 2, BudgetUsd = 900, Preferences = "{}"
        };
        db.AddRange(user, tourist, trip);
        await db.SaveChangesAsync();
        _tripId = trip.Id;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private AppDbContext Db() => PostgresFixture.CreateContext(_connectionString);

    private Quotation NewQuotation(int version) => new()
    {
        TripRequestId = _tripId, Version = version, SubtotalLkr = 100, MarginPct = 15, TotalLkr = 115, TotalUsd = 0.38m,
        FxRate = 300, FxAsOf = DateTime.UtcNow
    };

    private static async Task<string> SqlStateOf(Func<Task> save) =>
        (await save.Should().ThrowAsync<DbUpdateException>()).Which.InnerException.Should().BeOfType<PostgresException>()
        .Which.SqlState;

    [Fact]
    public async Task A_version_number_is_unique_per_trip()
    {
        await using (var db = Db())
        {
            db.Quotations.Add(NewQuotation(1));
            await db.SaveChangesAsync();
        }
        await using var second = Db();
        second.Quotations.Add(NewQuotation(1));
        (await SqlStateOf(() => second.SaveChangesAsync())).Should().Be(PostgresErrorCodes.UniqueViolation);
    }

    [Fact]
    public async Task Totals_fx_rate_and_line_types_are_checked()
    {
        await using var db = Db();
        var bad = NewQuotation(2);
        bad.TotalLkr = 50; // below the subtotal
        db.Quotations.Add(bad);
        (await SqlStateOf(() => db.SaveChangesAsync())).Should().Be(PostgresErrorCodes.CheckViolation);

        await using var lines = Db();
        var quotation = NewQuotation(3);
        quotation.Lines.Add(new QuotationLine { QuotationId = quotation.Id, LineType = "tip", Description = "Tip", Qty = 1, UnitLkr = 1, AmountLkr = 1 });
        lines.Quotations.Add(quotation);
        (await SqlStateOf(() => lines.SaveChangesAsync())).Should().Be(PostgresErrorCodes.CheckViolation);
    }
}
