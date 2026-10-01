using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TripCraft.Application.Trips;
using TripCraft.Application.Trips.Dtos;
using TripCraft.Application.Workflows;
using TripCraft.Tests.Common;

namespace TripCraft.Tests.Trips;

/// <summary>PLAN.md section 5: when the agent service is down the workflow ends FailedSafely, never a 500.</summary>
public class TripPlanningSafeFailureTests
{
    [Fact]
    public async Task Agent_service_failure_marks_workflow_failed_safely_and_restores_trip_status()
    {
        var failingAgent = new Mock<IAgentServiceClient>();
        // The real client never throws: it marks the workflow FailedSafely and returns false.
        failingAgent
            .Setup(a => a.StartAsync(It.IsAny<AgentWorkflow>(), It.IsAny<StartAgentWorkflowRequest>(), It.IsAny<CancellationToken>()))
            .Callback<AgentWorkflow, StartAgentWorkflowRequest, CancellationToken>((w, _, _) =>
            {
                w.Status = AgentWorkflowStatus.FailedSafely;
                w.ErrorSummary = "Agent service unavailable: connection refused";
                w.FinishedAt = DateTime.UtcNow;
            })
            .ReturnsAsync(false);

        await using var factory = new TestWebApplicationFactory();
        var app = factory.WithWebHostBuilder(b => b.ConfigureServices(s =>
            s.AddScoped(_ => failingAgent.Object)));

        var tourist = app.CreateClient();
        var touristToken = await TokenFor(app, "tourist1@tripcraft.test");
        tourist.DefaultRequestHeaders.Authorization = new("Bearer", touristToken);
        var created = await tourist.PostAsJsonAsync("/api/trip-requests", TripRequestsEndpointsTests.NewTrip());
        var trip = (await created.Content.ReadFromJsonAsync<TripRequestDto>(TestJson.Options))!;

        var response = await tourist.PostAsync($"/api/trip-requests/{trip.Id}/start-planning", null);

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var result = await response.Content.ReadFromJsonAsync<StartPlanningResponse>(TestJson.Options);
        result!.WorkflowStatus.Should().Be("FailedSafely");
        result.TripStatus.Should().Be("FailedSafely");
        result.ErrorSummary.Should().Contain("connection refused");

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TripCraft.Infrastructure.Persistence.AppDbContext>();
        var workflow = await db.AgentWorkflows.SingleAsync(w => w.Id == result.WorkflowId);
        workflow.Status.Should().Be(AgentWorkflowStatus.FailedSafely);
        workflow.FinishedAt.Should().NotBeNull();
        (await db.TripRequests.SingleAsync(t => t.Id == trip.Id)).Status.Should().Be(TripRequestStatus.FailedSafely);
        (await db.AuditLogs.AnyAsync(a => a.Action == "AgentWorkflowFailedSafely")).Should().BeTrue();

        // "Try again": a FailedSafely trip may start planning again (here the agent fails again, safely).
        var retry = await tourist.PostAsync($"/api/trip-requests/{trip.Id}/start-planning", null);
        retry.StatusCode.Should().Be(HttpStatusCode.Accepted);
        (await db.AgentWorkflows.CountAsync(w => w.TripRequestId == trip.Id)).Should().Be(2);
    }

    private static async Task<string> TokenFor(Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> app, string email)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TripCraft.Infrastructure.Persistence.AppDbContext>();
        var user = await db.Users.SingleAsync(u => u.Email == email);
        return scope.ServiceProvider.GetRequiredService<TripCraft.Application.Identity.ITokenService>().CreateToken(user).Token;
    }
}
