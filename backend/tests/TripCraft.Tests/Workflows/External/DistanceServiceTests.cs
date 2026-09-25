using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TripCraft.Application.Trips;
using TripCraft.Application.Workflows.External;
using TripCraft.Infrastructure.External;
using TripCraft.Infrastructure.Persistence;
using TripCraft.Infrastructure.Persistence.Repositories;

namespace TripCraft.Tests.Workflows.External;

public class DistanceServiceTests
{
    private readonly AppDbContext _db = new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase($"distance-{Guid.NewGuid()}").Options);

    public DistanceServiceTests()
    {
        _db.Attractions.AddRange(
            new Attraction { Name = "Temple", City = "Kandy", Latitude = 7.29, Longitude = 80.64 },
            new Attraction { Name = "Bridge", City = "Ella", Latitude = 6.87, Longitude = 81.06 });
        _db.CityDistances.Add(new CityDistance { FromCity = "Kandy", ToCity = "Ella", DistanceKm = 140, DurationMinutes = 270 });
        _db.SaveChanges();
    }

    private DistanceService Service(StubHandler handler, string? key = "ors-test-key") =>
        new(handler.Client(), _db, new AttractionRepository(_db),
            key is null ? StubHandler.Config() : StubHandler.Config(("ORS_API_KEY", key)),
            NullLogger<DistanceService>.Instance);

    [Fact]
    public async Task Provider_error_falls_back_to_the_static_table_in_either_direction()
    {
        var handler = StubHandler.Status(HttpStatusCode.ServiceUnavailable);

        var result = await Service(handler).GetDistanceAsync("Ella", "Kandy", CancellationToken.None);

        result.Should().Be(new DistanceResult("Ella", "Kandy", 140, 270, "static-table"));
        handler.Requests.Should().ContainSingle();
    }

    [Fact]
    public async Task Network_exception_falls_back_to_the_static_table()
    {
        var result = await Service(StubHandler.Throws(new TaskCanceledException("timeout")))
            .GetDistanceAsync("Kandy", "Ella", CancellationToken.None);

        result!.Source.Should().Be("static-table");
    }

    [Fact]
    public async Task No_key_skips_the_provider()
    {
        var handler = StubHandler.Json("{}");

        var result = await Service(handler, key: null).GetDistanceAsync("Kandy", "Ella", CancellationToken.None);

        result!.Source.Should().Be("static-table");
        handler.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task Unknown_pair_returns_null_without_throwing()
    {
        var result = await Service(StubHandler.Status(HttpStatusCode.BadGateway)).GetDistanceAsync("Kandy", "Jaffna", CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Success_uses_openrouteservice_with_the_key_in_a_header()
    {
        var handler = StubHandler.Json("""{"distances":[[0,142.6],[142.6,0]],"durations":[[0,15600],[15600,0]]}""");

        var result = await Service(handler).GetDistanceAsync("Kandy", "Ella", CancellationToken.None);

        result.Should().Be(new DistanceResult("Kandy", "Ella", 142.6m, 260, "openrouteservice"));
        handler.Requests[0].Headers.GetValues("Authorization").Should().Equal("ors-test-key");
        handler.Requests[0].RequestUri!.ToString().Should().NotContain("ors-test-key");
    }
}
