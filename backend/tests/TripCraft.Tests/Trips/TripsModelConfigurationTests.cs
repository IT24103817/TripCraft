using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using TripCraft.Application.Trips;
using TripCraft.Infrastructure.Persistence;

namespace TripCraft.Tests.Trips;

/// <summary>
/// Checks the Component A schema against PLAN.md section 4 by reading the EF Core model
/// built for Npgsql. No database connection is opened. Uses the design-time model because the
/// runtime model drops migration-only details such as check constraints.
/// </summary>
public class TripsModelConfigurationTests
{
    private static readonly IModel Model = BuildModel();

    private static IModel BuildModel()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=unused")
            .UseSnakeCaseNamingConvention()
            .Options;
        using var db = new AppDbContext(options);
        return db.GetService<IDesignTimeModel>().Model;
    }

    private static IEntityType Entity<T>() => Model.FindEntityType(typeof(T))!;

    private static string ColumnType<T>(string property) => Entity<T>().FindProperty(property)!.GetColumnType();

    private static bool HasUniqueIndex<T>(params string[] properties) =>
        Entity<T>().GetIndexes().Any(i => i.IsUnique && i.Properties.Select(p => p.Name).SequenceEqual(properties));

    [Theory]
    [InlineData(typeof(Tourist), "tourists")]
    [InlineData(typeof(TripRequest), "trip_requests")]
    [InlineData(typeof(Attraction), "attractions")]
    [InlineData(typeof(Itinerary), "itineraries")]
    [InlineData(typeof(ItineraryDay), "itinerary_days")]
    [InlineData(typeof(ItineraryStop), "itinerary_stops")]
    public void Entity_maps_to_table_from_plan(Type entity, string table)
    {
        Model.FindEntityType(entity)!.GetTableName().Should().Be(table);
    }

    [Fact]
    public void Unique_constraints_match_plan()
    {
        HasUniqueIndex<Tourist>(nameof(Tourist.UserId)).Should().BeTrue();
        HasUniqueIndex<Itinerary>(nameof(Itinerary.TripRequestId)).Should().BeTrue();
        HasUniqueIndex<ItineraryDay>(nameof(ItineraryDay.ItineraryId), nameof(ItineraryDay.DayNumber)).Should().BeTrue();
        HasUniqueIndex<ItineraryStop>(nameof(ItineraryStop.ItineraryDayId), nameof(ItineraryStop.Sequence)).Should().BeTrue();
    }

    [Fact]
    public void Trip_request_has_date_and_pax_check_constraints()
    {
        var checks = Entity<TripRequest>().GetCheckConstraints().Select(c => c.Sql).ToList();

        checks.Should().Contain("end_date >= start_date").And.Contain("pax > 0");
    }

    [Fact]
    public void Lookup_indexes_match_plan()
    {
        Entity<TripRequest>().GetIndexes().Should().Contain(i =>
            i.Properties.Select(p => p.Name).SequenceEqual(new[] { nameof(TripRequest.Status), nameof(TripRequest.StartDate) }));
        Entity<Attraction>().GetIndexes().Should().Contain(i =>
            i.Properties.Select(p => p.Name).SequenceEqual(new[] { nameof(Attraction.City) }));
    }

    [Fact]
    public void Column_types_match_plan()
    {
        ColumnType<TripRequest>(nameof(TripRequest.Id)).Should().Be("uuid");
        ColumnType<TripRequest>(nameof(TripRequest.Objective)).Should().Be("text");
        ColumnType<TripRequest>(nameof(TripRequest.StartDate)).Should().Be("date");
        ColumnType<TripRequest>(nameof(TripRequest.BudgetUsd)).Should().Be("numeric(12,2)");
        ColumnType<TripRequest>(nameof(TripRequest.Preferences)).Should().Be("jsonb");
        ColumnType<TripRequest>(nameof(TripRequest.CreatedAt)).Should().Be("timestamp with time zone");
        ColumnType<Attraction>(nameof(Attraction.EntryFeeLkr)).Should().Be("numeric(12,2)");
        ColumnType<ItineraryStop>(nameof(ItineraryStop.ArrivalTime)).Should().Be("time");
    }
}
