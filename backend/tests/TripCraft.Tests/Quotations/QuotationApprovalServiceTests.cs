using TripCraft.Application.Workflows.Dtos;
using FluentAssertions;
using Moq;
using TripCraft.Application.Common;
using TripCraft.Application.Common.Auditing;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Common.Notifications;
using TripCraft.Application.Common.Security;
using TripCraft.Application.Common.Settings;
using TripCraft.Application.Identity;
using TripCraft.Application.Quotations;
using TripCraft.Application.Trips;
using TripCraft.Application.Workflows;
using TripCraft.Application.Workflows.Ports;
using TripCraft.Tests.Workflows;

namespace TripCraft.Tests.Quotations;

/// <summary>Unit tests of the manager's review decisions (v1.1) with every dependency mocked.</summary>
public class QuotationApprovalServiceTests
{
    private static readonly CurrentUser Manager = new(Guid.NewGuid(), UserRole.OperationsManager);
    private static readonly DateOnly Start = new(2026, 10, 10);

    private readonly Mock<IQuotationStore> _quotations = new();
    private readonly Mock<IAgentWorkflowRepository> _workflows = new();
    private readonly Mock<ITripRequestRepository> _trips = new();
    private readonly Mock<IAgentServiceClient> _agent = new();
    private readonly Mock<INotifier> _notifier = new();
    private readonly Mock<IAuditLogger> _audit = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly TripRequest _trip = new() { StartDate = Start, EndDate = Start.AddDays(4), Pax = 4, Status = TripRequestStatus.PendingReview };
    private readonly AgentWorkflow _workflow = new() { Status = AgentWorkflowStatus.PendingApproval };
    private readonly Guid _quotationId = Guid.NewGuid();
    private readonly WorkflowOutcome _outcome;

    public QuotationApprovalServiceTests()
    {
        var proposal = TestProposals.Golden(Start, DemoAttractions.Random);
        _workflow.TripRequestId = _trip.Id;
        _outcome = new WorkflowOutcome(new StoredProposal(proposal.Days, proposal.Resources, proposal.Quotation, [], 0, _quotationId), null);
        _workflow.FinalOutcome = WorkflowJson.Serialize(_outcome);
        var summary = new QuotationSummary(_quotationId, _trip.Id, 1, "Pending", 187220, 624.07m, null);
        _quotations.Setup(q => q.GetAsync(_quotationId, It.IsAny<CancellationToken>())).ReturnsAsync(summary);
        _quotations.Setup(q => q.GetLatestForTripAsync(_trip.Id, It.IsAny<CancellationToken>())).ReturnsAsync(summary);
        _trips.Setup(t => t.GetByIdAsync(_trip.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_trip);
        _workflows.Setup(w => w.GetLatestForTripAsync(_trip.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_workflow);
    }

    private QuotationApprovalService Service() => new(_quotations.Object, _workflows.Object, _trips.Object,
        _agent.Object, _notifier.Object, TripSettings.Default with { LlmProvider = "groq" }, Mock.Of<IUserRepository>(),
        Mock.Of<IEmailDispatcher>(), _audit.Object, _unitOfWork.Object);

    [Fact]
    public async Task Send_to_client_moves_the_trip_to_QuotationSent_notifies_the_tourist_and_places_no_holds()
    {
        var result = await Service().ApproveAsync(Manager, _quotationId, "Looks good", CancellationToken.None);

        result.TripStatus.Should().Be("QuotationSent");
        result.HoldsCreated.Should().Be(0);
        _workflow.Status.Should().Be(AgentWorkflowStatus.Approved);
        _quotations.Verify(q => q.SetStatusAsync(_quotationId, QuotationDecision.Approved, It.IsAny<CancellationToken>()), Times.Once);
        _notifier.Verify(n => n.NotifyTourist(_trip, "QuotationSent", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        _audit.Verify(a => a.Record(Manager.Id, "TripRequestStatusChanged", nameof(TripRequest), _trip.Id,
            It.IsAny<object>(), It.IsAny<object>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task An_over_budget_proposal_cannot_be_sent()
    {
        _workflow.Status = AgentWorkflowStatus.RevisionRequested; // only Soft violations: the manager must revise or edit

        var act = () => Service().ApproveAsync(Manager, _quotationId, null, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("*over budget*");
        _trip.Status.Should().Be(TripRequestStatus.PendingReview);
    }

    [Fact]
    public async Task An_edited_proposal_must_be_repriced_before_it_is_sent()
    {
        _workflow.FinalOutcome = WorkflowJson.Serialize(_outcome with { EditedSinceQuotation = true });

        var act = () => Service().ApproveAsync(Manager, _quotationId, null, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("*Re-price*");
    }

    [Fact]
    public async Task An_older_version_cannot_be_decided()
    {
        _quotations.Setup(q => q.GetLatestForTripAsync(_trip.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QuotationSummary(Guid.NewGuid(), _trip.Id, 2, "Pending", 1, 1, null));

        var act = () => Service().ApproveAsync(Manager, _quotationId, null, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("Version 2 replaced*");
    }

    [Fact]
    public async Task A_trip_that_is_not_in_review_returns_409_from_the_state_machine()
    {
        _trip.Status = TripRequestStatus.Planning;

        var act = () => Service().ApproveAsync(Manager, _quotationId, null, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("A trip that is planning cannot become quotation sent.");
    }

    [Fact]
    public async Task Reject_cancels_the_trip_with_the_reason_in_the_history()
    {
        object? after = null;
        _audit.Setup(a => a.Record(Manager.Id, "TripRequestStatusChanged", nameof(TripRequest), _trip.Id, It.IsAny<object>(), It.IsAny<object>()))
            .Callback<Guid?, string, string, Guid, object?, object?>((_, _, _, _, _, a) => after = a);

        await Service().RejectAsync(Manager, _quotationId, "Dates fully booked", CancellationToken.None);

        _trip.Status.Should().Be(TripRequestStatus.Cancelled);
        _workflow.Status.Should().Be(AgentWorkflowStatus.Rejected);
        after.Should().BeEquivalentTo(new { Status = "Cancelled", Reason = "Rejected by the operator: Dates fully booked" });
    }

    [Fact]
    public async Task Request_revision_saves_first_then_calls_the_planner_and_a_failed_replan_fails_the_trip_safely()
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
        _trip.Status.Should().Be(TripRequestStatus.FailedSafely);
        _audit.Verify(a => a.Record(Manager.Id, "AgentWorkflowFailedSafely", nameof(AgentWorkflow), _workflow.Id,
            It.IsAny<object>(), It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task Request_revision_sends_the_comment_violations_and_cities_to_the_planner()
    {
        _trip.Cities = TripRequest.JoinCities(["Kandy", "Ella"]);
        _workflow.ValidationResult = WorkflowJson.Serialize(new ProposalValidationResult(false,
            [new ProposalRuleViolation("OVER_BUDGET", "Total USD 624.07 is over the budget of USD 400.", ViolationSeverity.Soft)]));
        StartAgentWorkflowRequest? sent = null;
        _agent.Setup(a => a.ReplanAsync(_workflow, It.IsAny<StartAgentWorkflowRequest>(), "Cheaper hotels", It.IsAny<CancellationToken>()))
            .Callback<AgentWorkflow, StartAgentWorkflowRequest, string, CancellationToken>((_, r, _, _) => sent = r)
            .ReturnsAsync(true);

        await Service().RequestRevisionAsync(Manager, _quotationId, "Cheaper hotels", CancellationToken.None);

        _trip.Status.Should().Be(TripRequestStatus.RevisionRequested);
        sent!.PreviousViolations.Should().ContainSingle()
            .Which.Should().Be(new PreviousViolation("OVER_BUDGET", "Total USD 624.07 is over the budget of USD 400."));
        sent.Cities.Should().Equal("Kandy", "Ella");
        sent.LlmProvider.Should().Be("groq", "the replan uses the provider chosen in Settings");
    }

    [Fact]
    public async Task A_quotation_the_client_declined_can_still_be_revised()
    {
        _quotations.Setup(q => q.GetAsync(_quotationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QuotationSummary(_quotationId, _trip.Id, 1, "Declined", 187220, 624.07m, null));
        _agent.Setup(a => a.ReplanAsync(_workflow, It.IsAny<StartAgentWorkflowRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Service().RequestRevisionAsync(Manager, _quotationId, "Client wants a beach day", CancellationToken.None);

        _trip.Status.Should().Be(TripRequestStatus.RevisionRequested);
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
