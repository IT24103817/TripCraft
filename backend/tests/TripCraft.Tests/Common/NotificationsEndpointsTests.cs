using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TripCraft.Application.Common.Notifications;
using TripCraft.Tests.Workflows;

namespace TripCraft.Tests.Common;

/// <summary>GET /api/notifications/mine and marking them read (v1.1).</summary>
public class NotificationsEndpointsTests
{
    [Fact]
    public async Task Each_user_sees_only_their_notifications_and_can_mark_them_read()
    {
        await using var factory = new TestWebApplicationFactory();
        await factory.RunToProposalAsync(); // the managers get "ReviewNeeded"
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);
        var tourist = await factory.CreateClientAsAsync(WorkflowFlow.Tourist);

        var mine = (await manager.GetFromJsonAsync<NotificationListDto>("/api/notifications/mine", TestJson.Options))!;
        var touristList = (await tourist.GetFromJsonAsync<NotificationListDto>("/api/notifications/mine", TestJson.Options))!;

        mine.UnreadCount.Should().Be(1);
        var item = mine.Items.Should().ContainSingle(n => n.Type == "ReviewNeeded").Subject;
        item.IsRead.Should().BeFalse();
        touristList.Items.Should().BeEmpty();

        (await tourist.PostAsync($"/api/notifications/{item.Id}/read", null)).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await manager.PostAsync($"/api/notifications/{item.Id}/read", null)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await manager.GetFromJsonAsync<NotificationListDto>("/api/notifications/mine", TestJson.Options))!.UnreadCount.Should().Be(0);
    }

    [Fact]
    public async Task Read_all_clears_the_count_and_anonymous_callers_get_401()
    {
        await using var factory = new TestWebApplicationFactory();
        await factory.RunToConfirmedAsync(); // the tourist gets "QuotationSent" and "TripConfirmed"
        var tourist = await factory.CreateClientAsAsync(WorkflowFlow.Tourist);

        var before = (await tourist.GetFromJsonAsync<NotificationListDto>("/api/notifications/mine", TestJson.Options))!;
        (await tourist.PostAsync("/api/notifications/read-all", null)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        var after = (await tourist.GetFromJsonAsync<NotificationListDto>("/api/notifications/mine", TestJson.Options))!;

        before.UnreadCount.Should().Be(2);
        before.Items.Select(n => n.Type).Should().Contain(["QuotationSent", "TripConfirmed"]);
        after.UnreadCount.Should().Be(0);
        (await factory.CreateClient().GetAsync("/api/notifications/mine")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
