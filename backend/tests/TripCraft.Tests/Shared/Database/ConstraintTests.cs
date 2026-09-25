using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using TripCraft.Application.Identity;
using TripCraft.Application.Trips;
using TripCraft.Application.Workflows;
using TripCraft.Application.Workflows.External;
using TripCraft.Infrastructure.Persistence;

namespace TripCraft.Tests.Shared.Database;

/// <summary>The database itself rejects bad rows, even if a bug slipped past the validators.</summary>
[Collection(PostgresCollection.Name)]
public class ConstraintTests(PostgresFixture postgres) : IAsyncLifetime
{
    private const string CheckViolation = PostgresErrorCodes.CheckViolation;   // 23514
    private const string UniqueViolation = PostgresErrorCodes.UniqueViolation; // 23505
    private string _connectionString = string.Empty;

    public async Task InitializeAsync() => _connectionString = await postgres.CreateMigratedDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private AppDbContext Db() => PostgresFixture.CreateContext(_connectionString);

    private static User NewUser(string email) =>
        new() { Email = email, FullName = "Test", PasswordHash = "hash", Role = UserRole.Tourist, IsActive = true };

    private async Task<Tourist> TouristAsync()
    {
        await using var db = Db();
        var user = NewUser($"{Guid.NewGuid():N}@tripcraft.test");
        var tourist = new Tourist { UserId = user.Id, Nationality = "UK", PassportNumberMasked = "****4567" };
        db.AddRange(user, tourist);
        await db.SaveChangesAsync();
        return tourist;
    }

    private static TripRequest Trip(Guid touristId, int pax = 4, int days = 5) => new()
    {
        TouristId = touristId, Objective = "5 days in Kandy and Ella", StartDate = new DateOnly(2026, 10, 10),
        EndDate = new DateOnly(2026, 10, 10).AddDays(days - 1), Pax = pax, BudgetUsd = 1500, Preferences = "{}"
    };

    private static async Task<string> SqlStateOf(Func<Task> save)
    {
        var error = await save.Should().ThrowAsync<DbUpdateException>();
        return error.Which.InnerException.Should().BeOfType<PostgresException>().Which.SqlState;
    }

    [Fact]
    public async Task Trip_with_zero_pax_violates_the_check_constraint()
    {
        var tourist = await TouristAsync();
        await using var db = Db();
        db.TripRequests.Add(Trip(tourist.Id, pax: 0));

        (await SqlStateOf(() => db.SaveChangesAsync())).Should().Be(CheckViolation);
    }

    [Fact]
    public async Task Trip_ending_before_it_starts_violates_the_check_constraint()
    {
        var tourist = await TouristAsync();
        await using var db = Db();
        db.TripRequests.Add(Trip(tourist.Id, days: -1));

        (await SqlStateOf(() => db.SaveChangesAsync())).Should().Be(CheckViolation);
    }

    [Fact]
    public async Task Duplicate_user_email_violates_the_unique_index()
    {
        await using (var first = Db())
        {
            first.Users.Add(NewUser("same@tripcraft.test"));
            await first.SaveChangesAsync();
        }
        await using var db = Db();
        db.Users.Add(NewUser("same@tripcraft.test"));

        (await SqlStateOf(() => db.SaveChangesAsync())).Should().Be(UniqueViolation);
    }

    [Fact]
    public async Task A_second_tourist_profile_for_the_same_user_is_rejected()
    {
        var tourist = await TouristAsync();
        await using var db = Db();
        db.Tourists.Add(new Tourist { UserId = tourist.UserId, Nationality = "UK", PassportNumberMasked = "****1111" });

        (await SqlStateOf(() => db.SaveChangesAsync())).Should().Be(UniqueViolation);
    }

    [Fact]
    public async Task Two_itinerary_days_with_the_same_number_are_rejected()
    {
        var tourist = await TouristAsync();
        await using var db = Db();
        var trip = Trip(tourist.Id);
        var itinerary = new Itinerary { TripRequestId = trip.Id, GeneratedBy = ItinerarySource.Agent };
        itinerary.Days.Add(new ItineraryDay { DayNumber = 1, City = "Kandy" });
        itinerary.Days.Add(new ItineraryDay { DayNumber = 1, City = "Ella" });
        db.AddRange(trip, itinerary);

        (await SqlStateOf(() => db.SaveChangesAsync())).Should().Be(UniqueViolation);
    }

    [Fact]
    public async Task Duplicate_agent_step_number_in_a_workflow_is_rejected()
    {
        var tourist = await TouristAsync();
        await using var db = Db();
        var trip = Trip(tourist.Id);
        var workflow = new AgentWorkflow { TripRequestId = trip.Id, Objective = trip.Objective, StartedAt = DateTime.UtcNow };
        db.AddRange(trip, workflow,
            new AgentStep { WorkflowId = workflow.Id, StepNo = 1, AgentName = "planner", Status = "Succeeded" },
            new AgentStep { WorkflowId = workflow.Id, StepNo = 1, AgentName = "itinerary", Status = "Succeeded" });

        (await SqlStateOf(() => db.SaveChangesAsync())).Should().Be(UniqueViolation);
    }

    [Fact]
    public async Task City_distance_must_be_positive_and_unique_per_pair()
    {
        await using (var db = Db())
        {
            db.CityDistances.Add(new CityDistance { FromCity = "Kandy", ToCity = "Ella", DistanceKm = 0, DurationMinutes = 270 });
            (await SqlStateOf(() => db.SaveChangesAsync())).Should().Be(CheckViolation);
        }
        await using (var db = Db())
        {
            db.CityDistances.Add(new CityDistance { FromCity = "Kandy", ToCity = "Ella", DistanceKm = 140, DurationMinutes = 270 });
            await db.SaveChangesAsync();
        }
        await using (var db = Db())
        {
            db.CityDistances.Add(new CityDistance { FromCity = "Kandy", ToCity = "Ella", DistanceKm = 141, DurationMinutes = 275 });
            (await SqlStateOf(() => db.SaveChangesAsync())).Should().Be(UniqueViolation);
        }
    }

    [Fact]
    public async Task Invalid_json_is_rejected_by_a_jsonb_column()
    {
        var tourist = await TouristAsync();
        await using var db = Db();
        var trip = Trip(tourist.Id);
        trip.Preferences = "{not json";
        db.TripRequests.Add(trip);

        (await SqlStateOf(() => db.SaveChangesAsync())).Should().Be(PostgresErrorCodes.InvalidTextRepresentation);
    }
}
