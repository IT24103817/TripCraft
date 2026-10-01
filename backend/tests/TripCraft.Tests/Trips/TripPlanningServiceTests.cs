using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TripCraft.Application.Common;
using TripCraft.Application.Common.Auditing;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Common.Security;
using TripCraft.Application.Common.Settings;
using TripCraft.Application.Identity;
using TripCraft.Application.Trips;
using TripCraft.Application.Trips.Services;
using TripCraft.Application.Workflows;

namespace TripCraft.Tests.Trips;

/// <summary>
/// Unit tests for the Component A business operation (start-planning) with every dependency mocked.
/// No database, no HTTP — only the service's decisions.
/// </summary>
public class TripPlanningServiceTests
{
    private static readonly DateOnly Start = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(10);
    private static readonly CurrentUser Manager = new(Guid.NewGuid(), UserRole.OperationsManager);

    private readonly Mock<ITripRequestRepository> _trips = new();
    private readonly Mock<IAttractionRepository> _attractions = new();
    private readonly Mock<IAgentWorkflowRepository> _workflows = new();
    private readonly Mock<IAgentServiceClient> _agent = new();
    private readonly Mock<IAuditLogger> _audit = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    public TripPlanningServiceTests()
    {
        _attractions.Setup(a => a.ListActiveCitiesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(["Colombo", "Ella", "Galle", "Kandy"]);
        _agent.Setup(a => a.StartAsync(It.IsAny<AgentWorkflow>(), It.IsAny<StartAgentWorkflowRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    private TripPlanningService CreateService() => new(
        _trips.Object, _attractions.Object, _workflows.Object, _agent.Object,
        TripSettings.Default with { LlmProvider = "groq" }, _audit.Object, _unitOfWork.Object, NullLogger<TripPlanningService>.Instance);

    private TripRequest GivenTrip(string objective, DateOnly start, DateOnly end,
        TripRequestStatus status = TripRequestStatus.Submitted)
    {
        var trip = new TripRequest
        {
            Objective = objective, StartDate = start, EndDate = end, Pax = 4, BudgetUsd = 1500,
            Status = status, Tourist = new Tourist { UserId = Guid.NewGuid(), PassportNumberMasked = "****4521" }
        };
        _trips.Setup(t => t.GetByIdAsync(trip.Id, It.IsAny<CancellationToken>())).ReturnsAsync(trip);
        return trip;
    }

    [Fact]
    public async Task The_chosen_cities_and_the_LLM_provider_from_settings_go_to_the_agents()
    {
        var trip = GivenTrip("A quiet week, no city named", Start, Start.AddDays(2));
        trip.Cities = TripRequest.JoinCities(["Galle", "Colombo"]);
        StartAgentWorkflowRequest? sent = null;
        _agent.Setup(a => a.StartAsync(It.IsAny<AgentWorkflow>(), It.IsAny<StartAgentWorkflowRequest>(), It.IsAny<CancellationToken>()))
            .Callback<AgentWorkflow, StartAgentWorkflowRequest, CancellationToken>((_, r, _) => sent = r).ReturnsAsync(true);

        await CreateService().StartPlanningAsync(Manager, trip.Id, CancellationToken.None);

        sent!.Cities.Should().Equal("Galle", "Colombo");
        sent.LlmProvider.Should().Be("groq");
    }

    [Fact]
    public async Task Happy_path_creates_workflow_moves_trip_to_planning_and_calls_agent_after_saving()
    {
        var trip = GivenTrip("5 days in Kandy and Ella", Start, Start.AddDays(4));
        AgentWorkflow? saved = null;
        _workflows.Setup(w => w.Add(It.IsAny<AgentWorkflow>())).Callback<AgentWorkflow>(w => saved = w);
        var calls = new List<string>();
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => calls.Add("save")).Returns(Task.CompletedTask);
        _agent.Setup(a => a.StartAsync(It.IsAny<AgentWorkflow>(), It.IsAny<StartAgentWorkflowRequest>(), It.IsAny<CancellationToken>()))
            .Callback(() => calls.Add("agent")).ReturnsAsync(true);

        var result = await CreateService().StartPlanningAsync(Manager, trip.Id, CancellationToken.None);

        result.WorkflowStatus.Should().Be("Planning");
        trip.Status.Should().Be(TripRequestStatus.Planning);
        saved!.TripRequestId.Should().Be(trip.Id);
        result.Skeleton.Select(d => d.City).Should().Equal("Kandy", "Kandy", "Kandy", "Ella", "Ella");
        // The DB commit must happen before the HTTP call to the agent service.
        calls.Should().Equal("save", "agent");
        _audit.Verify(a => a.Record(Manager.Id, "TripRequestStatusChanged", nameof(TripRequest), trip.Id,
            It.IsAny<object>(), It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task Boundary_one_day_trip_to_one_city_is_accepted()
    {
        var trip = GivenTrip("A single day in Galle", Start, Start);

        var result = await CreateService().StartPlanningAsync(Manager, trip.Id, CancellationToken.None);

        result.Skeleton.Should().ContainSingle().Which.City.Should().Be("Galle");
    }

    [Fact]
    public async Task Boundary_exactly_thirty_days_is_accepted()
    {
        var trip = GivenTrip("A month around Colombo, Kandy, Ella and Galle", Start, Start.AddDays(29));

        var result = await CreateService().StartPlanningAsync(Manager, trip.Id, CancellationToken.None);

        result.Skeleton.Should().HaveCount(30);
    }

    [Fact]
    public async Task Rejects_more_cities_than_days_with_validation_error_and_saves_nothing()
    {
        var trip = GivenTrip("One day covering Kandy and Ella", Start, Start);

        var act = () => CreateService().StartPlanningAsync(Manager, trip.Id, CancellationToken.None);

        (await act.Should().ThrowAsync<ValidationException>())
            .Which.Errors.Should().Contain(e => e.ErrorMessage.Contains("mentions 2 cities"));
        trip.Status.Should().Be(TripRequestStatus.Submitted);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _agent.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Rejects_trip_that_is_already_being_planned_with_conflict()
    {
        var trip = GivenTrip("5 days in Kandy and Ella", Start, Start.AddDays(4), TripRequestStatus.Planning);

        var act = () => CreateService().StartPlanningAsync(Manager, trip.Id, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        _workflows.Verify(w => w.Add(It.IsAny<AgentWorkflow>()), Times.Never);
    }

    [Fact]
    public async Task Rejects_tourist_starting_someone_elses_trip_with_forbidden()
    {
        var trip = GivenTrip("5 days in Kandy and Ella", Start, Start.AddDays(4));
        var otherTourist = new CurrentUser(Guid.NewGuid(), UserRole.Tourist);

        var act = () => CreateService().StartPlanningAsync(otherTourist, trip.Id, CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
