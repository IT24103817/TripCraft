using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Resources.Dtos;
using TripCraft.Application.Workflows.Ports;
using TripCraft.Infrastructure.Resources;
using TripCraft.Tests.Common;

namespace TripCraft.Tests.Resources;

/// <summary>Component B business operations over HTTP: availability search, manual block (409 on overlap), release.</summary>
public class AvailabilityAndHoldsEndpointsTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    private static readonly DateOnly Start = new(2027, 3, 1);

    [Fact]
    public async Task A_manual_block_removes_the_van_from_availability_until_it_is_released()
    {
        var client = await factory.CreateClientAsAsync("manager1@tripcraft.test");
        var url = $"/api/availability?type=Vehicle&from={Start:yyyy-MM-dd}&to={Start.AddDays(2):yyyy-MM-dd}&seats=4";

        var before = await client.GetFromJsonAsync<List<AvailableResourceDto>>(url, TestJson.Options);
        before!.Should().Contain(v => v.Id == ResourcesSeeder.VanSixSeats);

        var block = await client.PostAsJsonAsync("/api/resource-holds",
            new CreateHoldRequest(ResourceType.Vehicle, ResourcesSeeder.VanSixSeats, Start.AddDays(1), Start.AddDays(1), 1, "Service"));
        block.StatusCode.Should().Be(HttpStatusCode.Created);
        var hold = (await block.Content.ReadFromJsonAsync<HoldDto>(TestJson.Options))!;
        hold.ResourceName.Should().Be("CAB-1234");

        (await client.GetFromJsonAsync<List<AvailableResourceDto>>(url, TestJson.Options))!
            .Should().NotContain(v => v.Id == ResourcesSeeder.VanSixSeats);
        (await client.PostAsJsonAsync("/api/resource-holds",
                new CreateHoldRequest(ResourceType.Vehicle, ResourcesSeeder.VanSixSeats, Start, Start.AddDays(3), 1, null)))
            .StatusCode.Should().Be(HttpStatusCode.Conflict);

        var calendar = await client.GetFromJsonAsync<PagedResult<HoldDto>>(
            $"/api/resource-holds?from={Start:yyyy-MM-dd}&to={Start.AddDays(5):yyyy-MM-dd}&type=Vehicle", TestJson.Options);
        calendar!.Items.Should().Contain(h => h.Id == hold.Id && h.Note == "Service");

        (await client.PostAsync($"/api/resource-holds/{hold.Id}/release", null)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.GetFromJsonAsync<List<AvailableResourceDto>>(url, TestJson.Options))!
            .Should().Contain(v => v.Id == ResourcesSeeder.VanSixSeats);
        (await client.PostAsync($"/api/resource-holds/{hold.Id}/release", null)).StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Room_availability_needs_the_rooms_on_every_night_and_reports_the_rate()
    {
        var client = await factory.CreateClientAsAsync("manager1@tripcraft.test");

        var rooms = await client.GetFromJsonAsync<List<AvailableResourceDto>>(
            $"/api/availability?type=Room&from={Start:yyyy-MM-dd}&to={Start.AddDays(1):yyyy-MM-dd}&city=Ella&rooms=4", TestJson.Options);

        rooms!.Should().ContainSingle(r => r.Id == ResourcesSeeder.EllaStandard).Which.RateLkr.Should().Be(12000m);
        rooms.Should().NotContain(r => r.Name.Contains("Deluxe"), "only 3 deluxe rooms exist");
    }

    [Fact]
    public async Task Guide_availability_needs_language_and_pax_and_bad_queries_are_400()
    {
        var client = await factory.CreateClientAsAsync("manager1@tripcraft.test");

        var guides = await client.GetFromJsonAsync<List<AvailableResourceDto>>(
            $"/api/availability?type=Guide&from={Start:yyyy-MM-dd}&to={Start.AddDays(4):yyyy-MM-dd}&language=de&pax=4", TestJson.Options);
        guides!.Should().ContainSingle().Which.Name.Should().Be("Kumari Silva");

        (await client.GetAsync($"/api/availability?type=Guide&from={Start:yyyy-MM-dd}&to={Start:yyyy-MM-dd}"))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await client.GetAsync($"/api/availability?type=Vehicle&from={Start.AddDays(3):yyyy-MM-dd}&to={Start:yyyy-MM-dd}&seats=2"))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Tourists_cannot_search_or_block_resources()
    {
        var tourist = await factory.CreateClientAsAsync("tourist1@tripcraft.test");

        (await tourist.GetAsync($"/api/availability?type=Vehicle&from={Start:yyyy-MM-dd}&to={Start:yyyy-MM-dd}&seats=2"))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await tourist.PostAsJsonAsync("/api/resource-holds",
                new CreateHoldRequest(ResourceType.Vehicle, ResourcesSeeder.VanSixSeats, Start, Start, 1, null)))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
