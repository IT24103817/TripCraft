using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TripCraft.Application.Trips.Dtos;
using TripCraft.Application.Workflows.Dtos;
using TripCraft.Tests.Common;
using TripCraft.Tests.Trips;

namespace TripCraft.Tests.Workflows;

/// <summary>Steps of the PLAN.md section 6 flow, shared by the workflow and approval tests.</summary>
public static class WorkflowFlow
{
    public const string Tourist = "tourist1@tripcraft.test";
    public const string OtherTourist = "tourist2@tripcraft.test";
    public const string Manager = "manager1@tripcraft.test";

    public static HttpClient CreateInternalClient(this TestWebApplicationFactory factory)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Internal-Key", TestWebApplicationFactory.InternalKey);
        return client;
    }

    public static T State<T>(this TestWebApplicationFactory factory) where T : notnull =>
        factory.Services.GetRequiredService<T>();

    /// <summary>Tourist submits the demo trip and starts planning. Returns the trip and the workflow id.</summary>
    public static async Task<(TripRequestDto Trip, Guid WorkflowId)> StartPlanningAsync(
        this TestWebApplicationFactory factory, decimal budgetUsd = 1500)
    {
        var tourist = await factory.CreateClientAsAsync(Tourist);
        var created = await tourist.PostAsJsonAsync("/api/trip-requests", TripRequestsEndpointsTests.NewTrip(budget: budgetUsd));
        created.EnsureSuccessStatusCode();
        var trip = (await created.Content.ReadFromJsonAsync<TripRequestDto>(TestJson.Options))!;

        var started = await tourist.PostAsync($"/api/trip-requests/{trip.Id}/start-planning", null);
        started.EnsureSuccessStatusCode();
        var result = (await started.Content.ReadFromJsonAsync<StartPlanningResponse>(TestJson.Options))!;
        return (trip, result.WorkflowId);
    }

    public static Task<DemoAttractions> SeededAttractionsAsync(this TestWebApplicationFactory factory) =>
        factory.QueryDbAsync(async db =>
        {
            var ids = await db.Attractions.ToDictionaryAsync(a => a.Name, a => a.Id);
            return new DemoAttractions(ids["Temple of the Sacred Tooth Relic"], ids["Royal Botanical Gardens, Peradeniya"],
                ids["Nine Arches Bridge"], ids["Little Adam's Peak"]);
        });

    public static async Task<HttpResponseMessage> PostProposalAsync(this TestWebApplicationFactory factory, Guid workflowId,
        AgentProposalRequest proposal) =>
        await factory.CreateInternalClient().PostAsJsonAsync($"/api/internal/workflows/{workflowId}/proposal", proposal);

    /// <summary>Start planning and post the golden proposal. Returns the outcome (PendingApproval unless over budget).</summary>
    public static async Task<(TripRequestDto Trip, ProposalOutcomeResponse Outcome)> RunToProposalAsync(
        this TestWebApplicationFactory factory, decimal budgetUsd = 1500)
    {
        var (trip, workflowId) = await factory.StartPlanningAsync(budgetUsd);
        var proposal = TestProposals.Golden(trip.StartDate, await factory.SeededAttractionsAsync());
        var response = await factory.PostProposalAsync(workflowId, proposal);
        response.EnsureSuccessStatusCode();
        return (trip, (await response.Content.ReadFromJsonAsync<ProposalOutcomeResponse>(TestJson.Options))!);
    }

    /// <summary>Manager sends the quotation (PendingReview → QuotationSent).</summary>
    public static async Task SendToClientAsync(this TestWebApplicationFactory factory, Guid quotationId)
    {
        var manager = await factory.CreateClientAsAsync(Manager);
        (await manager.PostAsync($"/api/quotations/{quotationId}/approve", null)).EnsureSuccessStatusCode();
    }

    /// <summary>Proposal → sent → accepted by the tourist. Returns the trip in ClientAccepted.</summary>
    public static async Task<(TripRequestDto Trip, ProposalOutcomeResponse Outcome)> RunToClientAcceptedAsync(
        this TestWebApplicationFactory factory)
    {
        var (trip, outcome) = await factory.RunToProposalAsync();
        await factory.SendToClientAsync(outcome.QuotationId!.Value);
        var tourist = await factory.CreateClientAsAsync(Tourist);
        (await tourist.PostAsync($"/api/quotations/{outcome.QuotationId}/accept", null)).EnsureSuccessStatusCode();
        return (trip, outcome);
    }

    /// <summary>The whole booking: proposal → sent → accepted → confirmed.</summary>
    public static async Task<(TripRequestDto Trip, ProposalOutcomeResponse Outcome)> RunToConfirmedAsync(
        this TestWebApplicationFactory factory)
    {
        var (trip, outcome) = await factory.RunToClientAcceptedAsync();
        var manager = await factory.CreateClientAsAsync(Manager);
        (await manager.PostAsync($"/api/trip-requests/{trip.Id}/confirm", null)).EnsureSuccessStatusCode();
        return (trip, outcome);
    }
}
