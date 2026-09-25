using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TripCraft.Application.Workflows;
using TripCraft.Infrastructure.Workflows;

namespace TripCraft.Tests.Workflows.External;

public class AgentServiceClientTests
{
    private static readonly StartAgentWorkflowRequest Request = new(Guid.NewGuid(), Guid.NewGuid(), "5 days in Kandy",
        new DateOnly(2026, 10, 10), new DateOnly(2026, 10, 14), 4, 1500m, """{"language":"en"}""", []);

    private static AgentServiceClient Client(StubHandler handler, bool withUrl = true) =>
        new(withUrl ? handler.Client("http://agents.test/") : new HttpClient(handler),
            StubHandler.Config(("INTERNAL_AGENT_KEY", "agent-test-key")), NullLogger<AgentServiceClient>.Instance);

    [Fact]
    public async Task Connection_failure_marks_the_workflow_failed_safely_and_does_not_throw()
    {
        var workflow = new AgentWorkflow();

        var ok = await Client(StubHandler.Throws(new HttpRequestException("connection refused")))
            .StartAsync(workflow, Request, CancellationToken.None);

        ok.Should().BeFalse();
        workflow.Status.Should().Be(AgentWorkflowStatus.FailedSafely);
        workflow.ErrorSummary.Should().Be("Agent service unavailable: HttpRequestException");
        workflow.FinishedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Error_status_marks_the_workflow_failed_safely()
    {
        var workflow = new AgentWorkflow();

        var ok = await Client(StubHandler.Status(HttpStatusCode.Unauthorized)).StartAsync(workflow, Request, CancellationToken.None);

        ok.Should().BeFalse();
        workflow.ErrorSummary.Should().Contain("401");
    }

    [Fact]
    public async Task Missing_service_url_fails_safely_without_a_call()
    {
        var handler = StubHandler.Status(HttpStatusCode.Accepted);
        var workflow = new AgentWorkflow();

        var ok = await Client(handler, withUrl: false).StartAsync(workflow, Request, CancellationToken.None);

        ok.Should().BeFalse();
        workflow.ErrorSummary.Should().Contain("AGENT_SERVICE_URL");
        handler.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task Replan_posts_to_the_agent_with_the_key_and_the_manager_comment()
    {
        var handler = StubHandler.Status(HttpStatusCode.Accepted);
        var workflow = new AgentWorkflow();

        var ok = await Client(handler).ReplanAsync(workflow, Request, "cheaper hotels", CancellationToken.None);

        ok.Should().BeTrue();
        workflow.Status.Should().Be(AgentWorkflowStatus.Planning);
        handler.Requests[0].RequestUri!.ToString().Should().Be("http://agents.test/replan");
        handler.Requests[0].Headers.GetValues("X-Internal-Key").Should().Equal("agent-test-key");
        handler.Bodies[0].Should().Contain("\"managerComment\":\"cheaper hotels\"")
            .And.Contain($"\"workflowId\":\"{Request.WorkflowId}\"")
            .And.Contain("\"startDate\":\"2026-10-10\"")
            .And.Contain("\"preferencesJson\"");
    }
}
