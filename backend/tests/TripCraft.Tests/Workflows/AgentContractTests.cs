using System.Text.Json;
using FluentAssertions;
using TripCraft.Application.Workflows.Dtos;
using TripCraft.Application.Workflows.Validation;
using TripCraft.Tests.Common;

namespace TripCraft.Tests.Workflows;

/// <summary>
/// Contract between the two services: the fixtures are the exact JSON bodies the Python agent service
/// posts (captured from its golden pytest run). If either side renames a field, this test fails.
/// </summary>
public class AgentContractTests
{
    private static T Load<T>(string file) =>
        JsonSerializer.Deserialize<T>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Workflows", "Fixtures", file)),
            TestJson.Options)!;

    [Fact]
    public void Agent_step_report_maps_onto_the_step_dto_and_passes_validation()
    {
        var step = Load<AgentStepReportRequest>("agent_step.json");

        step.AgentName.Should().Be("planner");
        step.Status.Should().Be("Succeeded");
        step.ToolCalls!.Select(c => c.Tool).Should().Equal("parse_dates", "list_agents");
        step.InputSummary!.Value.GetProperty("pax").GetInt32().Should().Be(4);
        new AgentStepReportRequestValidator().Validate(step).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Agent_proposal_maps_onto_the_proposal_dto_and_passes_validation()
    {
        var proposal = Load<AgentProposalRequest>("agent_proposal.json");

        proposal.Status.Should().Be("PendingApproval");
        proposal.Days.Should().HaveCount(5);
        proposal.Days![2].Should().BeEquivalentTo(new { City = "Ella", Transport = "train", TransferKm = 140m, DrivingMinutes = 0 });
        proposal.Days[0].Stops![0].AttractionId.Should().Be("a-temple");
        proposal.Resources!.GuideId.Should().Be("g-1");
        proposal.Resources.Rooms.Should().HaveCount(8);
        proposal.Quotation!.TotalLkr.Should().Be(187220m);
        proposal.Quotation.TotalUsd.Should().Be(624.07m);
        proposal.Quotation.FxAsOf.Should().Be(new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc));
        proposal.Plan!.Value.GetProperty("constraints").GetProperty("cities").GetArrayLength().Should().Be(2);
        new AgentProposalRequestValidator().Validate(proposal).IsValid.Should().BeTrue();
    }
}
