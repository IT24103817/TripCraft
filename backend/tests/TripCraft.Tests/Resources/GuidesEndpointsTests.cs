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
        new(name, "+94 77 000 1111", languages.Length == 0 ? ["en", "it"] : [.. languages], 6200, 8, true);

    private static CreateGuideRequest NewGuideAccount(string email, string name = "Sunil Bandara") =>
        new(name, "+94 77 000 1111", ["en", "it"], 6200, 8, true, email);

    [Fact]
    public async Task Create_edit_list_and_delete_a_guide()
    {
        var client = await factory.CreateClientAsAsync(Manager);

        var created = await client.PostAsJsonAsync("/api/guides", NewGuideAccount("sunil.crud@tripcraft.test"));
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        var guide = (await created.Content.ReadFromJsonAsync<GuideAccountDto>(TestJson.Options))!.Guide;
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

        var response = await client.PostAsJsonAsync("/api/guides",
            NewGuideAccount("not-an-email") with { Languages = ["english"], MaxPax = 0 });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("two-letter language codes").And.Contain("MaxPax").And.Contain("Email");
    }

    private static async Task<TripCraft.Application.Identity.Dtos.AuthResponse> LoginAsync(HttpClient client, string email, string password)
    {
        var response = await AuthHelper.LoginAsync(client, email, password);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TripCraft.Application.Identity.Dtos.AuthResponse>(TestJson.Options))!;
    }

    [Fact]
    public async Task Creating_a_guide_creates_their_login_with_a_one_time_password_that_must_be_changed()
    {
        var manager = await factory.CreateClientAsAsync(Manager);

        var response = await manager.PostAsJsonAsync("/api/guides", NewGuideAccount("Kamal.New@TripCraft.test", "Kamal Silva"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var account = (await response.Content.ReadFromJsonAsync<GuideAccountDto>(TestJson.Options))!;
        account.Email.Should().Be("kamal.new@tripcraft.test");
        account.TemporaryPassword.Should().MatchRegex("^(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9]).{12}$");
        var (login, auditJson) = await factory.QueryDbAsync(async db => (
            await db.Users.SingleAsync(u => u.Email == "kamal.new@tripcraft.test"),
            string.Join(" ", await db.AuditLogs.Where(a => a.EntityId == account.Guide.Id).Select(a => a.After).ToListAsync())));
        login.Role.Should().Be(TripCraft.Application.Identity.UserRole.Guide);
        login.MustChangePassword.Should().BeTrue();
        account.Guide.UserId.Should().Be(login.Id);
        auditJson.Should().NotContain(account.TemporaryPassword, "the password is never written to the audit log");

        // First login: the app is told to force a change; after changing, the flag is cleared.
        var guide = factory.CreateClient();
        var auth = await LoginAsync(guide, "kamal.new@tripcraft.test", account.TemporaryPassword);
        auth.User.MustChangePassword.Should().BeTrue();
        guide.DefaultRequestHeaders.Authorization = new("Bearer", auth.AccessToken);
        var changed = await guide.PostAsJsonAsync("/api/auth/change-password",
            new TripCraft.Application.Identity.Dtos.ChangePasswordRequest(account.TemporaryPassword, "NewPassw0rd!"));
        changed.StatusCode.Should().Be(HttpStatusCode.OK);
        (await changed.Content.ReadFromJsonAsync<TripCraft.Application.Identity.Dtos.UserDto>(TestJson.Options))!
            .MustChangePassword.Should().BeFalse();
    }

    [Fact]
    public async Task Manager_can_reset_a_guide_password_and_the_old_one_stops_working()
    {
        var manager = await factory.CreateClientAsAsync(Manager);
        var account = (await (await manager.PostAsJsonAsync("/api/guides", NewGuideAccount("reset.me@tripcraft.test")))
            .Content.ReadFromJsonAsync<GuideAccountDto>(TestJson.Options))!;

        var reset = await manager.PostAsync($"/api/guides/{account.Guide.Id}/reset-password", null);

        reset.StatusCode.Should().Be(HttpStatusCode.OK);
        var fresh = (await reset.Content.ReadFromJsonAsync<GuideAccountDto>(TestJson.Options))!;
        fresh.TemporaryPassword.Should().NotBe(account.TemporaryPassword);
        var guide = factory.CreateClient();
        (await guide.PostAsJsonAsync("/api/auth/login", new { email = "reset.me@tripcraft.test", password = account.TemporaryPassword }))
            .StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await LoginAsync(guide, "reset.me@tripcraft.test", fresh.TemporaryPassword)).User.MustChangePassword.Should().BeTrue();
    }

    [Fact]
    public async Task A_guide_email_that_already_has_an_account_is_409()
    {
        var client = await factory.CreateClientAsAsync(Manager);

        (await client.PostAsJsonAsync("/api/guides", NewGuideAccount("tourist1@tripcraft.test")))
            .StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Change_password_with_a_wrong_current_password_is_400()
    {
        var client = await factory.CreateClientAsAsync("tourist2@tripcraft.test");

        var response = await client.PostAsJsonAsync("/api/auth/change-password",
            new TripCraft.Application.Identity.Dtos.ChangePasswordRequest("WrongPassw0rd", "NewPassw0rd!"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain("current password is not correct");
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
