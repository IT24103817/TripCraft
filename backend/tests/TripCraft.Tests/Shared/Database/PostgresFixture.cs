using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;
using TripCraft.Infrastructure.Persistence;

namespace TripCraft.Tests.Shared.Database;

/// <summary>
/// A real PostgreSQL server for the database tests (PLAN.md section 11, "Database").
/// - TEST_DATABASE_URL set (CI's postgres:16 service, or a local server): that server is used;
/// - otherwise a postgres:16 container is started with Testcontainers (needs Docker).
/// Every test class gets its own empty database, dropped at the end.
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _container;
    private readonly List<string> _databases = [];

    public string ServerConnectionString { get; private set; } = string.Empty;
    public string Source { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        var url = Environment.GetEnvironmentVariable("TEST_DATABASE_URL");
        if (!string.IsNullOrWhiteSpace(url))
        {
            ServerConnectionString = ConnectionStringParser.ToNpgsql(url);
            Source = "TEST_DATABASE_URL";
            return;
        }

        _container = new PostgreSqlBuilder("postgres:16-alpine").Build();
        await _container.StartAsync();
        ServerConnectionString = _container.GetConnectionString();
        Source = "Testcontainers postgres:16-alpine";
    }

    /// <summary>Creates an empty database with a unique name and returns its connection string.</summary>
    public async Task<string> CreateDatabaseAsync()
    {
        var name = $"tripcraft_test_{Guid.NewGuid():N}";
        await using (var connection = new NpgsqlConnection(ServerConnectionString))
        {
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand($"CREATE DATABASE \"{name}\"", connection);
            await command.ExecuteNonQueryAsync();
        }
        _databases.Add(name);
        return new NpgsqlConnectionStringBuilder(ServerConnectionString) { Database = name, Pooling = false }.ConnectionString;
    }

    /// <summary>An empty database with every migration applied.</summary>
    public async Task<string> CreateMigratedDatabaseAsync()
    {
        var connectionString = await CreateDatabaseAsync();
        await using var db = CreateContext(connectionString);
        await db.Database.MigrateAsync();
        return connectionString;
    }

    public static AppDbContext CreateContext(string connectionString) =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options);

    public async Task DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
            return;
        }
        // Shared server: remove the databases this run created.
        await using var connection = new NpgsqlConnection(ServerConnectionString);
        await connection.OpenAsync();
        foreach (var name in _databases)
        {
            await using var command = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{name}\" WITH (FORCE)", connection);
            await command.ExecuteNonQueryAsync();
        }
    }
}

[CollectionDefinition(Name)]
public class PostgresCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "Postgres";
}
