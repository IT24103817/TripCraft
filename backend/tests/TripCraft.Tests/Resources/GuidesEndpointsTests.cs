using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Resources.Dtos;
using TripCraft.Tests.Common;

namespace TripCraft.Tests.Resources;

/// <summary>One CRUD flow for Component B (PLAN.md section 11) plus search, filter, sort, paging and roles.</summary>
public class GuidesEndpointsTests(TestWebApplicationFactory factory) : IClassFixture<TestWebApplicationFactory>
{
    private const string Manager = "manager1@tripcraft.test";

    private static SaveGuideRequest NewGuide(string name = "Sunil Bandara", params string[] languages) =>
        new(name, "+94 77 000 1111", languages.Length == 0 ? ["en", "it"] : [.. languages], 6200, 8, true, null);

    [Fact]
    public async Task Create_edit_list_and_delete_a_guide()
    {
        var client = await factory.CreateClientAsAsync(Manager);

        var created = await client.PostAsJsonAsync("/api/guides", NewGuide());
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        var guide = (await created.Content.ReadFromJsonAsync<GuideDto>(TestJson.Options))!;
        guide.Languages.Should().Equal("en", "it");
        created.Headers.Location!.AbsolutePath.Should().Be($"/api/guides/{guide.Id}");

        var updated = await client.PutAsJsonAsync($"/api/guides/{guide.Id}", NewGuide("Sunil Bandara", "en", "fr") with { MaxPax = 12 });
        updated.StatusCode.Should().Be(HttpStatusCode.OK);
        var after = (await updated.Content.ReadFromJsonAsync<GuideDto>(TestJson.Options))!;
        after.Languages.Should().Equal("en", "fr");
        after.MaxPax.Should().Be(12);

        var page = await client.GetFromJsonAsync<PagedResult<GuideDto>>("/api/guides?search=sunil&language=fr", TestJson.Options);
        page!.Items.Should().ContainSingle(g => g.Id == guide.Id);

        (await client.DeleteAsync($"/api/guides/{guide.Id}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.GetAsync($"/api/guides/{guide.Id}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        var audit = await factory.QueryDbAsync(db => db.AuditLogs.Where(a => a.EntityId == guide.Id).Select(a => a.Action).ToListAsync());
        audit.Should().BeEquivalentTo(["GuideCreated", "GuideUpdated", "GuideDeleted"]);
    }

    [Fact]
    public async Task List_filters_by_language_sorts_and_pages()
    {
        var client = await factory.CreateClientAsAsync(Manager);

        var page = await client.GetFromJsonAsync<PagedResult<GuideDto>>(
            "/api/guides?language=en&sort=-dayRateLkr&page=1&pageSize=2", TestJson.Options);

        page!.Items.Should().HaveCount(2).And.BeInDescendingOrder(g => g.DayRateLkr);
        page.Total.Should().BeGreaterThanOrEqualTo(4);
        page.Items.Should().OnlyContain(g => g.Languages.Contains("en"));
    }

    [Fact]
    public async Task Invalid_guide_is_400_with_field_errors()
    {
        var client = await factory.CreateClientAsAsync(Manager);

        var response = await client.PostAsJsonAsync("/api/guides", NewGuide() with { Languages = ["english"], MaxPax = 0 });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("two-letter language codes").And.Contain("MaxPax");
    }

    [Fact]
    public async Task Linking_a_login_that_is_not_a_guide_or_already_linked_is_409()
    {
        var client = await factory.CreateClientAsAsync(Manager);
        var (touristId, linkedGuideUserId) = await factory.QueryDbAsync(async db => (
            (await db.Users.SingleAsync(u => u.Email == "tourist1@tripcraft.test")).Id,
            (await db.Users.SingleAsync(u => u.Email == "guide1@tripcraft.test")).Id));

        (await client.PostAsJsonAsync("/api/guides", NewGuide() with { UserId = touristId }))
            .StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await client.PostAsJsonAsync("/api/guides", NewGuide() with { UserId = linkedGuideUserId }))
            .StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Theory]
    [InlineData("tourist1@tripcraft.test")]
    [InlineData("guide1@tripcraft.test")]
    [InlineData("admin1@tripcraft.test")]
    public async Task Only_the_operations_manager_manages_guides(string email)
    {
        var client = await factory.CreateClientAsAsync(email);

        (await client.GetAsync("/api/guides")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await client.PostAsJsonAsync("/api/guides", NewGuide())).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
