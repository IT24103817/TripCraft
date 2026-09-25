using System.Text.Json;
using FluentAssertions;
using TripCraft.Application.Workflows.Dtos;
using TripCraft.Application.Workflows.Validation;

namespace TripCraft.Tests.Workflows;

public class WorkflowValidatorsTests
{
    private static AgentStepReportRequest Step(string agent = "planner", string status = "Succeeded", int retries = 0,
        JsonElement? output = null) => new(agent, [], null, output, null, 100, retries, status);

    [Fact]
    public void Step_report_accepts_the_four_agents_and_rejects_anything_else()
    {
        var validator = new AgentStepReportRequestValidator();
        foreach (var agent in new[] { "planner", "itinerary", "resources", "validation" })
            validator.Validate(Step(agent)).IsValid.Should().BeTrue();

        validator.Validate(Step("manager")).Errors.Should().ContainSingle(e => e.PropertyName == "AgentName");
        validator.Validate(Step(status: "Approved")).Errors.Should().ContainSingle(e => e.PropertyName == "Status");
        validator.Validate(Step(retries: 11)).Errors.Should().ContainSingle(e => e.PropertyName == "Retries");
    }

    [Fact]
    public void Step_summaries_over_8000_characters_are_rejected()
    {
        var big = JsonDocument.Parse(JsonSerializer.Serialize(new { text = new string('x', 8001) })).RootElement;

        new AgentStepReportRequestValidator().Validate(Step(output: big)).Errors
            .Should().ContainSingle(e => e.PropertyName == "OutputSummary");
    }

    [Theory]
    [InlineData("PendingApproval", true)]
    [InlineData("RevisionRequested", true)]
    [InlineData("FailedSafely", true)]
    [InlineData("Approved", false)]
    [InlineData("Confirmed", false)]
    public void Proposal_status_must_be_one_the_agents_may_report(string status, bool valid)
    {
        var proposal = new AgentProposalRequest(null, null, null, null, null, status, 0, null);

        new AgentProposalRequestValidator().Validate(proposal).IsValid.Should().Be(valid);
    }

    [Fact]
    public void Tool_queries_reject_bad_cities_languages_and_ranges()
    {
        new CityQueryValidator().Validate(new CityQuery { City = "Kandy" }).IsValid.Should().BeTrue();
        new CityQueryValidator().Validate(new CityQuery { City = "Kandy'; DROP TABLE x;--" }).IsValid.Should().BeFalse();
        new GuideAvailabilityQueryValidator().Validate(new GuideAvailabilityQuery
        {
            From = new DateOnly(2026, 10, 14), To = new DateOnly(2026, 10, 10), Language = "English", Pax = 0
        }).Errors.Select(e => e.PropertyName).Should().BeEquivalentTo(["To", "Language", "Pax"]);
        new RoomAvailabilityQueryValidator().Validate(new RoomAvailabilityQuery
        {
            City = "Ella", Night = new DateOnly(2026, 10, 12), Rooms = 31
        }).Errors.Should().ContainSingle(e => e.PropertyName == "Rooms");
    }

    [Fact]
    public void Workflow_list_paging_is_bounded()
    {
        var validator = new WorkflowListQueryValidator();
        validator.Validate(new WorkflowListQuery { Page = 1, PageSize = 100 }).IsValid.Should().BeTrue();
        validator.Validate(new WorkflowListQuery { Page = 0, PageSize = 101 }).Errors.Should().HaveCount(2);
    }
}
