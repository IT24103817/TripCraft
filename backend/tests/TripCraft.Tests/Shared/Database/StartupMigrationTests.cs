using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using TripCraft.Tests.Common;

namespace TripCraft.Tests.Shared.Database;

/// <summary>The container start-up path: RUN_MIGRATIONS=true on an empty database migrates, then seeds.</summary>
[Collection(PostgresCollection.Name)]
public class StartupMigrationTests(PostgresFixture postgres)
{
    private sealed class EmptyDatabaseFactory(string connectionString, bool runMigrations)
        : PostgresWebApplicationFactory(connectionString)
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.UseSetting("RUN_MIGRATIONS", runMigrations ? "true" : "false");
        }
    }

    [Fact]
    public async Task Run_migrations_true_migrates_and_seeds_an_empty_database_and_health_is_ok()
    {
        var connectionString = await postgres.CreateDatabaseAsync(); // empty: no tables at all
        await using var factory = new EmptyDatabaseFactory(connectionString, runMigrations: true);

        var health = await factory.CreateClient().GetAsync("/health");

        health.StatusCode.Should().Be(HttpStatusCode.OK);
        await using var db = PostgresFixture.CreateContext(connectionString);
        (await db.Database.GetPendingMigrationsAsync()).Should().BeEmpty();
        (await db.Users.CountAsync()).Should().Be(12);          // 3 per role
        (await db.Attractions.CountAsync()).Should().Be(21);
        (await db.CityDistances.CountAsync()).Should().Be(15);
    }

    [Fact]
    public async Task Starting_twice_does_not_seed_twice()
    {
        var connectionString = await postgres.CreateDatabaseAsync();
        await using (var first = new EmptyDatabaseFactory(connectionString, runMigrations: true))
            (await first.CreateClient().GetAsync("/health")).EnsureSuccessStatusCode();
        await using (var second = new EmptyDatabaseFactory(connectionString, runMigrations: true))
            (await second.CreateClient().GetAsync("/health")).EnsureSuccessStatusCode();

        await using var db = PostgresFixture.CreateContext(connectionString);
        (await db.Users.CountAsync()).Should().Be(12);
        (await db.Attractions.CountAsync()).Should().Be(21);
    }
}
