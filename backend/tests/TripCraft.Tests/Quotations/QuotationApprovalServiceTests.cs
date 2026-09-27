using TripCraft.Application.Workflows.Dtos;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TripCraft.Application.Common;
using TripCraft.Application.Common.Auditing;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Common.Security;
using TripCraft.Application.Identity;
using TripCraft.Application.Quotations;
using TripCraft.Application.Trips;
using TripCraft.Application.Workflows;
using TripCraft.Application.Workflows.Ports;
using TripCraft.Tests.Workflows;

namespace TripCraft.Tests.Quotations;

/// <summary>Unit tests of the approval business operation with every dependency mocked.</summary>
public class QuotationApprovalServiceTests
{
    private static readonly CurrentUser Manager = new(Guid.NewGuid(), UserRole.OperationsManager);
    private static readonly DateOnly Start = new(2026, 10, 10);

    private readonly Mock<IQuotationStore> _quotations = new();
    private readonly Mock<IResourceHoldService> _holds = new();
    private readonly Mock<IAgentWorkflowRepository> _workflows = new();
    private readonly Mock<ITripRequestRepository> _trips = new();
    private readonly Mock<IAgentServiceClient> _agent = new();
    private readonly Mock<IAuditLogger> _audit = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly TripRequest _trip = new() { StartDate = Start, EndDate = Start.AddDays(4), Pax = 4, Status = TripRequestStatus.PendingApproval };
    private readonly AgentWorkflow _workflow = new() { Status = AgentWorkflowStatus.PendingApproval };
    private readonly Guid _quotationId = Guid.NewGuid();

    public QuotationApprovalServiceTests()
    {
        var proposal = TestProposals.Golden(Start, DemoAttractions.Random);
        _workflow.TripRequestId = _trip.Id;
        _workflow.FinalOutcome = WorkflowJson.Serialize(new WorkflowOutcome(
            new StoredProposal(proposal.Days, proposal.Resources, proposal.Quotation, [], 0, _quotationId), null));
        _quotations.Setup(q => q.GetAsync(_quotationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QuotationSummary(_quotationId, _trip.Id, 1, true, 187220, 624.07m));
        _trips.Setup(t => t.GetByIdAsync(_trip.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_trip);
        _workflows.Setup(w => w.GetLatestForTripAsync(_trip.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_workflow);
        _unitOfWork.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<IUnitOfWorkTransaction>());
    }

    private QuotationApprovalService Service() => new(_quotations.Object, _holds.Object, _workflows.Object, _trips.Object,
        _agent.Object, _audit.Object, _unitOfWork.Object, NullLogger<QuotationApprovalService>.Instance);

    [Fact]
    public void Holds_are_one_per_guide_and_vehicle_and_one_per_room_type_and_night()
    {
        var outcome = WorkflowJson.Deserialize<WorkflowOutcome>(_workflow.FinalOutcome)!;

        var holds = QuotationApprovalService.BuildHolds(outcome.Proposal, _trip);

        holds.Should().HaveCount(6);
        holds.Where(h => h.Type != ResourceType.Room).Should().OnlyContain(h => h.From == Start && h.To == Start.AddDays(4));
        holds.Where(h => h.Type == ResourceType.Room).Select(h => (h.From, h.Quantity))
            .Should().Equal((Start, 2), (Start.AddDays(1), 2), (Start.AddDays(2), 2), (Start.AddDays(3), 2));
    }

    [Fact]
    public async Task Approve_commits_once_after_every_hold_and_decision()
    {
        var order = new List<string>();
        _holds.Setup(h => h.CreateHoldAsync(It.IsAny<ResourceHoldRequest>(), It.IsAny<CancellationToken>()))
            .Callback(() => order.Add("hold")).Returns(Task.CompletedTask);
        _quotations.Setup(q => q.RecordDecision(_quotationId, Manager.Id, QuotationDecision.Approved, null)).Callback(() => order.Add("decision"));
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Callback(() => order.Add("save")).Returns(Task.CompletedTask);

        var result = await Service().ApproveAsync(Manager, _quotationId, null, CancellationToken.None);

        result.HoldsCreated.Should().Be(6);
        _trip.Status.Should().Be(TripRequestStatus.Confirmed);
        _workflow.Status.Should().Be(AgentWorkflowStatus.Completed);
        order.Should().Equal("hold", "hold", "hold", "hold", "hold", "hold", "decision", "save");
    }

    [Fact]
    public async Task A_hold_conflict_discards_everything_and_never_saves()
    {
        _holds.SetupSequence(h => h.CreateHoldAsync(It.IsAny<ResourceHoldRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .ThrowsAsync(new ConflictException("Vehicle already held."));

        var act = () => Service().ApproveAsync(Manager, _quotationId, null, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("Vehicle already held.");
        _unitOfWork.Verify(u => u.DiscardChanges(), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _quotations.Verify(q => q.RecordDecision(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<QuotationDecision>(), It.IsAny<string?>()), Times.Never);
    }

    [Fact]
    public async Task Any_other_failure_is_rolled_back_and_reported_as_409()
    {
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new TimeoutException("db timeout"));

        var act = () => Service().ApproveAsync(Manager, _quotationId, null, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("*rolled back*");
        _unitOfWork.Verify(u => u.DiscardChanges(), Times.Once);
    }

    [Fact]
    public async Task Request_revision_saves_first_then_calls_the_planner_and_records_a_failed_replan()
    {
        var order = new List<string>();
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Callback(() => order.Add("save")).Returns(Task.CompletedTask);
        _agent.Setup(a => a.ReplanAsync(_workflow, It.IsAny<StartAgentWorkflowRequest>(), "Cheaper hotels", It.IsAny<CancellationToken>()))
            .Callback<AgentWorkflow, StartAgentWorkflowRequest, string, CancellationToken>((w, _, _, _) =>
            {
                order.Add("replan");
                w.Status = AgentWorkflowStatus.FailedSafely;
            })
            .ReturnsAsync(false);

        await Service().RequestRevisionAsync(Manager, _quotationId, "Cheaper hotels", CancellationToken.None);

        order.Should().Equal("save", "replan", "save");
        _trip.Status.Should().Be(TripRequestStatus.RevisionRequested);
        _audit.Verify(a => a.Record(Manager.Id, "AgentWorkflowFailedSafely", nameof(AgentWorkflow), _workflow.Id,
            It.IsAny<object>(), It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task Request_revision_sends_the_rejected_proposals_violations_to_the_planner()
    {
        _workflow.ValidationResult = WorkflowJson.Serialize(new ProposalValidationResult(false,
            [new ProposalRuleViolation("OVER_BUDGET", "Total USD 624.07 is over the budget of USD 400.", ViolationSeverity.Soft)]));
        StartAgentWorkflowRequest? sent = null;
        _agent.Setup(a => a.ReplanAsync(_workflow, It.IsAny<StartAgentWorkflowRequest>(), "Cheaper hotels", It.IsAny<CancellationToken>()))
            .Callback<AgentWorkflow, StartAgentWorkflowRequest, string, CancellationToken>((_, r, _, _) => sent = r)
            .ReturnsAsync(true);

        await Service().RequestRevisionAsync(Manager, _quotationId, "Cheaper hotels", CancellationToken.None);

        sent!.PreviousViolations.Should().ContainSingle()
            .Which.Should().Be(new PreviousViolation("OVER_BUDGET", "Total USD 624.07 is over the budget of USD 400."));
    }

    [Fact]
    public void Previous_violations_are_empty_without_a_stored_validation_result()
    {
        PreviousViolation.FromValidationJson(null).Should().BeEmpty();
        PreviousViolation.FromValidationJson("""{"isValid":true,"violations":[]}""").Should().BeEmpty();
    }

    [Fact]
    public async Task A_completed_workflow_cannot_be_rejected()
    {
        _workflow.Status = AgentWorkflowStatus.Completed;

        var act = () => Service().RejectAsync(Manager, _quotationId, null, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
