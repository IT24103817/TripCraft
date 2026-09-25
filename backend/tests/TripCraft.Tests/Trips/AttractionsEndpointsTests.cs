using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Trips.Dtos;
using TripCraft.Tests.Common;

namespace TripCraft.Tests.Trips;

public class AttractionsEndpointsTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    private const string Manager = "manager1@tripcraft.test";

    private static SaveAttractionRequest NewAttraction(string name) =>
        new(name, "Kandy", "Museum", 90, 1500, 7.2931, 80.6350);

    [Fact]
    public async Task Manager_can_create_read_update_and_delete_an_attraction()
    {
        var client = await factory.CreateClientAsAsync(Manager);

        var created = await client.PostAsJsonAsync("/api/attractions", NewAttraction("Kandy Museum"));
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        var attraction = await created.Content.ReadFromJsonAsync<AttractionDto>(TestJson.Options);
        created.Headers.Location!.AbsolutePath.Should().Be($"/api/attractions/{attraction!.Id}");

        var updated = await client.PutAsJsonAsync($"/api/attractions/{attraction.Id}", NewAttraction("Kandy National Museum"));
        updated.StatusCode.Should().Be(HttpStatusCode.OK);
        (await updated.Content.ReadFromJsonAsync<AttractionDto>(TestJson.Options))!.Name.Should().Be("Kandy National Museum");

        var deleted = await client.DeleteAsync($"/api/attractions/{attraction.Id}");
        deleted.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var afterDelete = await client.GetAsync($"/api/attractions/{attraction.Id}");
        afterDelete.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Duplicate_name_in_the_same_city_returns_409()
    {
        var client = await factory.CreateClientAsAsync(Manager);

        var response = await client.PostAsJsonAsync("/api/attractions",
            NewAttraction("Temple of the Sacred Tooth Relic"));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Invalid_attraction_returns_400()
    {
        var client = await factory.CreateClientAsAsync(Manager);

        var response = await client.PostAsJsonAsync("/api/attractions", NewAttraction("") with { DurationMinutes = 0 });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain("Name").And.Contain("DurationMinutes");
    }

    [Fact]
    public async Task Tourist_can_read_but_not_create_attractions()
    {
        var client = await factory.CreateClientAsAsync("tourist1@tripcraft.test");

        (await client.GetAsync("/api/attractions")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.PostAsJsonAsync("/api/attractions", NewAttraction("Nope"))).StatusCode
            .Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task List_filters_by_city_and_sorts_by_entry_fee()
    {
        var client = await factory.CreateClientAsAsync(Manager);

        var page = await client.GetFromJsonAsync<PagedResult<AttractionDto>>(
            "/api/attractions?city=kandy&sort=-entryFeeLkr", TestJson.Options);

        page!.Items.Should().NotBeEmpty().And.OnlyContain(a => a.City == "Kandy");
        page.Items.Should().BeInDescendingOrder(a => a.EntryFeeLkr);
    }
}
