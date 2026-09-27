using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TripCraft.Application.Quotations;
using TripCraft.Tests.Common;
using TripCraft.Tests.Workflows;

namespace TripCraft.Tests.Quotations;

/// <summary>Every status code of the approval endpoints (200/400/401/403/404/409).</summary>
public class QuotationStatusCodeTests
{
    [Fact]
    public async Task Unknown_quotation_returns_404_for_every_decision()
    {
        await using var factory = new TestWebApplicationFactory();
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);
        var missing = Guid.NewGuid();

        (await manager.PostAsync($"/api/quotations/{missing}/approve", null)).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await manager.PostAsync($"/api/quotations/{missing}/reject", null)).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await manager.PostAsJsonAsync($"/api/quotations/{missing}/request-revision", new RequestRevisionRequest("x")))
            .StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task No_token_is_401_and_a_guide_is_403()
    {
        await using var factory = new TestWebApplicationFactory();
        var (_, outcome) = await factory.RunToProposalAsync();
        var guide = await factory.CreateClientAsAsync("guide1@tripcraft.test");

        (await factory.CreateClient().PostAsync($"/api/quotations/{outcome.QuotationId}/approve", null))
            .StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await guide.PostAsync($"/api/quotations/{outcome.QuotationId}/approve", null))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Approving_an_over_budget_proposal_is_409_but_rejecting_it_is_200()
    {
        await using var factory = new TestWebApplicationFactory();
        var (_, outcome) = await factory.RunToProposalAsync(budgetUsd: 400); // RevisionRequested
        var manager = await factory.CreateClientAsAsync(WorkflowFlow.Manager);

        (await manager.PostAsync($"/api/quotations/{outcome.QuotationId}/approve", null)).StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await manager.PostAsync($"/api/quotations/{outcome.QuotationId}/reject", null)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await manager.PostAsync($"/api/quotations/{outcome.QuotationId}/reject", null)).StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Theory]
    [InlineData("tourist1@tripcraft.test")]
    [InlineData("guide1@tripcraft.test")]
    [InlineData("admin1@tripcraft.test")]
    public async Task Only_the_operations_manager_decides_or_recalculates(string email)
    {
        await using var factory = new TestWebApplicationFactory();
        var client = await factory.CreateClientAsAsync(email);
        var id = Guid.NewGuid();

        (await client.PostAsJsonAsync($"/api/quotations/{id}/reject", new QuotationDecisionRequest("no")))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await client.PostAsJsonAsync($"/api/quotations/{id}/request-revision", new RequestRevisionRequest("cheaper")))
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
