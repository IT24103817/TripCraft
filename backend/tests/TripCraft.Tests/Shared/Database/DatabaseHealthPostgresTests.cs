using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using TripCraft.Infrastructure.Persistence;

namespace TripCraft.Tests.Shared.Database;

/// <summary>GET /health's database ping on real PostgreSQL: pooled, fast, and "fail" (not an exception) when down.</summary>
[Collection(PostgresCollection.Name)]
public class DatabaseHealthPostgresTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Many_checks_succeed_on_pooled_connections()
    {
        var connectionString = new NpgsqlConnectionStringBuilder(await postgres.CreateMigratedDatabaseAsync())
            { Pooling = true, MaxPoolSize = 5 }.ConnectionString;

        for (var i = 0; i < 200; i++)
        {
            await using var db = PostgresFixture.CreateContext(connectionString);
            (await new DatabaseHealth(db).CanConnectAsync(CancellationToken.None)).Should().BeTrue();
        }
    }

    [Fact]
    public async Task An_unreachable_database_is_reported_as_down_without_throwing()
    {
        var connectionString = new NpgsqlConnectionStringBuilder(postgres.ServerConnectionString)
            { Port = 1, Timeout = 2 }.ConnectionString;
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString).UseSnakeCaseNamingConvention().Options);

        (await new DatabaseHealth(db).CanConnectAsync(CancellationToken.None)).Should().BeFalse();
    }
}
