using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;

namespace TripCraft.Tests.Shared.Database;

[Collection(PostgresCollection.Name)]
public class MigrationsTests(PostgresFixture postgres)
{
    [Fact]
    public async Task All_migrations_apply_to_an_empty_database_and_create_every_table()
    {
        var connectionString = await postgres.CreateDatabaseAsync();
        await using var db = PostgresFixture.CreateContext(connectionString);

        await db.Database.MigrateAsync();

        (await db.Database.GetPendingMigrationsAsync()).Should().BeEmpty();
        (await db.Database.GetAppliedMigrationsAsync()).Should().Equal(db.Database.GetMigrations());
        var tables = await TablesAsync(connectionString);
        tables.Should().Contain([
            "users", "tourists", "trip_requests", "attractions", "itineraries", "itinerary_days", "itinerary_stops",
            "agent_workflows", "agent_steps", "audit_logs", "city_distances"
        ]);
    }

    [Fact]
    public async Task Jsonb_money_and_timestamp_columns_have_the_planned_types()
    {
        var connectionString = await postgres.CreateMigratedDatabaseAsync();

        var types = await ColumnTypesAsync(connectionString);

        types[("trip_requests", "preferences")].Should().Be("jsonb");
        types[("trip_requests", "budget_usd")].Should().Be("numeric(12,2)");
        types[("trip_requests", "id")].Should().Be("uuid");
        types[("trip_requests", "created_at")].Should().Be("timestamp with time zone");
        types[("agent_workflows", "final_outcome")].Should().Be("jsonb");
        types[("agent_workflows", "validation_result")].Should().Be("jsonb");
        types[("agent_steps", "input_summary")].Should().Be("jsonb");
    }

#pragma warning disable EF1001 // The model differ is internal EF API, used here only to catch a forgotten migration.
    [Fact]
    public void The_migrations_match_the_current_model()
    {
        using var db = PostgresFixture.CreateContext("Host=unused");
        var snapshot = db.GetService<IMigrationsAssembly>().ModelSnapshot!.Model;
        if (snapshot is IMutableModel mutable)
            snapshot = mutable.FinalizeModel();
        snapshot = db.GetService<IModelRuntimeInitializer>().Initialize(snapshot);

        var differences = db.GetService<IMigrationsModelDiffer>().GetDifferences(
            snapshot.GetRelationalModel(), db.GetService<IDesignTimeModel>().Model.GetRelationalModel());

        differences.Should().BeEmpty("every model change needs a migration (dotnet ef migrations add ...)");
    }
#pragma warning restore EF1001

    private static async Task<List<string>> TablesAsync(string connectionString)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'", connection);
        await using var reader = await command.ExecuteReaderAsync();
        var tables = new List<string>();
        while (await reader.ReadAsync())
            tables.Add(reader.GetString(0));
        return tables;
    }

    private static async Task<Dictionary<(string, string), string>> ColumnTypesAsync(string connectionString)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand("""
            SELECT c.relname, a.attname, format_type(a.atttypid, a.atttypmod)
            FROM pg_attribute a JOIN pg_class c ON c.oid = a.attrelid JOIN pg_namespace n ON n.oid = c.relnamespace
            WHERE n.nspname = 'public' AND a.attnum > 0 AND NOT a.attisdropped
            """, connection);
        await using var reader = await command.ExecuteReaderAsync();
        var types = new Dictionary<(string, string), string>();
        while (await reader.ReadAsync())
            types[(reader.GetString(0), reader.GetString(1))] = reader.GetString(2);
        return types;
    }
}
