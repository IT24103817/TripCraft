using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TripCraft.Application.Quotations;
using TripCraft.Tests.Common;
using TripCraft.Tests.Workflows;

namespace TripCraft.Tests.Quotations;

/// <summary>Every status code of the operator and client quotation endpoints (200/400/401/403/404/409).</summary>
public class QuotationStatusCodeTests
{
    [Fact]
    public async Task Unknown_quotation_or_trip_returns_404_for_every_operator_action()
    {
        await using var factory = new TestWebApplicationFactory();
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);
        var missing = Guid.NewGuid();

        (await manager.PostAsync($"/api/quotations/{missing}/send", null)).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await manager.PostAsJsonAsync($"/api/trip-requests/{missing}/replan", new ReplanRequest("x")))
            .StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await manager.PostAsync($"/api/trip-requests/{missing}/confirm", null)).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task No_token_is_401_and_a_guide_is_403()
    {
        await using var factory = new TestWebApplicationFactory();
        var (trip, outcome) = await factory.RunToClientAcceptedAsync();
        var guide = await factory.CreateClientAsAsync("guide1@tripcraft.test");

        (await factory.CreateClient().PostAsync($"/api/trip-requests/{trip.Id}/confirm", null))
            .StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await guide.PostAsync($"/api/trip-requests/{trip.Id}/confirm", null)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await guide.PostAsync($"/api/quotations/{outcome.QuotationId}/send", null)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Sending_a_quotation_that_is_already_sent_is_409()
    {
        await using var factory = new TestWebApplicationFactory();
        var (_, outcome) = await factory.RunToProposalAsync(); // auto-sent
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);

        (await manager.PostAsync($"/api/quotations/{outcome.QuotationId}/send", null)).StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Theory]
    [InlineData("tourist1@tripcraft.test")]
    [InlineData("guide1@tripcraft.test")]
    [InlineData("admin1@tripcraft.test")]
    public async Task Only_the_operations_manager_sends_replans_or_recalculates(string email)
    {
        await using var factory = new TestWebApplicationFactory();
        var client = await factory.CreateClientAsAsync(email);
        var id = Guid.NewGuid();

        (await client.PostAsJsonAsync($"/api/quotations/{id}/send", new QuotationDecisionRequest("x")))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await client.PostAsJsonAsync($"/api/trip-requests/{id}/replan", new ReplanRequest("cheaper")))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await client.PostAsync($"/api/quotations/{id}/calculate", null)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Only_the_tourist_accepts_a_quotation_and_only_a_guide_checks_in()
    {
        await using var factory = new TestWebApplicationFactory();
        var manager = await factory.CreateClientAsAsync("manager1@tripcraft.test");
        var tourist = await factory.CreateClientAsAsync("tourist1@tripcraft.test");

        (await manager.PostAsync($"/api/quotations/{Guid.NewGuid()}/accept", null)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await tourist.PostAsJsonAsync("/api/check-ins", new { itineraryStopId = Guid.NewGuid(), latitude = 7.29, longitude = 80.64 }))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
