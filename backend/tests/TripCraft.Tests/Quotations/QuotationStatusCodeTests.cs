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
}
