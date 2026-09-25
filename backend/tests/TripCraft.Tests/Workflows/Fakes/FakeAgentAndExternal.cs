using TripCraft.Application.Workflows;
using TripCraft.Application.Workflows.External;

namespace TripCraft.Tests.Workflows.Fakes;

/// <summary>Records calls to the agent service. Set Fail = true to simulate the service being down.</summary>
public class FakeAgentState
{
    public bool Fail { get; set; }
    public List<(string Kind, Guid WorkflowId, string? Comment)> Calls { get; } = [];
}

public class FakeAgentServiceClient(FakeAgentState state) : IAgentServiceClient
{
    public Task<bool> StartAsync(AgentWorkflow workflow, StartAgentWorkflowRequest request, CancellationToken ct) =>
        Task.FromResult(Record(workflow, "start", null));

    public Task<bool> ReplanAsync(AgentWorkflow workflow, StartAgentWorkflowRequest request, string managerComment,
        CancellationToken ct) => Task.FromResult(Record(workflow, "replan", managerComment));

    private bool Record(AgentWorkflow workflow, string kind, string? comment)
    {
        state.Calls.Add((kind, workflow.Id, comment));
        if (!state.Fail)
            return true;
        workflow.Status = AgentWorkflowStatus.FailedSafely;
        workflow.ErrorSummary = "Agent service unavailable: connection refused";
        workflow.FinishedAt = DateTime.UtcNow;
        return false;
    }
}

public class FakeExchangeRateService : IExchangeRateService
{
    public Task<ExchangeRate> GetUsdToLkrAsync(CancellationToken ct) =>
        Task.FromResult(new ExchangeRate("USD", "LKR", 300m, new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc), false));
}

public class FakeDistanceService : IDistanceService
{
    public Task<DistanceResult?> GetDistanceAsync(string fromCity, string toCity, CancellationToken ct) =>
        Task.FromResult<DistanceResult?>(new[] { fromCity, toCity }.Order().SequenceEqual(["Ella", "Kandy"])
            ? new DistanceResult(fromCity, toCity, 140m, 270, "fake")
            : null);
}

public class FakeWeatherService : IWeatherService
{
    public Task<WeatherForecast?> GetForecastAsync(string city, DateOnly date, CancellationToken ct) =>
        Task.FromResult<WeatherForecast?>(null);
}
