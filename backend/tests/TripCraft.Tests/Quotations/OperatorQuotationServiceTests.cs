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

/// <summary>
/// The operator's exceptions to the automatic flow (v1.1), with every dependency mocked: Send after an edit
/// (the client must accept again) and Replan with note after a decline.
/// </summary>
public class OperatorQuotationServiceTests
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
    private readonly TripRequest _trip = new() { StartDate = Start, EndDate = Start.AddDays(4), Pax = 4, Status = TripRequestStatus.ClientAccepted };
    private readonly AgentWorkflow _workflow = new() { Status = AgentWorkflowStatus.PendingApproval };
    private readonly Guid _quotationId = Guid.NewGuid();
    private readonly WorkflowOutcome _outcome;

    public OperatorQuotationServiceTests()
    {
        var proposal = TestProposals.Golden(Start, DemoAttractions.Random);
        _workflow.TripRequestId = _trip.Id;
        _outcome = new WorkflowOutcome(new StoredProposal(proposal.Days, proposal.Resources, proposal.Quotation, [], 0, _quotationId), null);
        _workflow.FinalOutcome = WorkflowJson.Serialize(_outcome);
        var summary = new QuotationSummary(_quotationId, _trip.Id, 2, "Pending", 187220, 624.07m, null);
        _quotations.Setup(q => q.GetAsync(_quotationId, It.IsAny<CancellationToken>())).ReturnsAsync(summary);
        _quotations.Setup(q => q.GetLatestForTripAsync(_trip.Id, It.IsAny<CancellationToken>())).ReturnsAsync(summary);
        _trips.Setup(t => t.GetByIdAsync(_trip.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_trip);
        _workflows.Setup(w => w.GetLatestForTripAsync(_trip.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_workflow);
    }

    private OperatorQuotationService Service() => new(_quotations.Object, _workflows.Object, _trips.Object, _agent.Object,
        _notifier.Object, TripSettings.Default with { LlmProvider = "groq" }, Mock.Of<IUserRepository>(),
        Mock.Of<IEmailDispatcher>(), _audit.Object, _unitOfWork.Object);

    [Fact]
    public async Task Edit_and_resend_sends_the_new_version_and_the_client_must_accept_again()
    {
        var result = await Service().SendAsync(Manager, _quotationId, "Swapped to the coach", CancellationToken.None);

        result.TripStatus.Should().Be("QuotationSent");
        _quotations.Verify(q => q.SetStatusAsync(_quotationId, QuotationDecision.Approved, It.IsAny<CancellationToken>()), Times.Once);
        _notifier.Verify(n => n.NotifyTourist(_trip, "QuotationUpdated", "Your quote was updated, please review", It.IsAny<string>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task An_edit_that_was_not_re_priced_cannot_be_sent()
    {
        _workflow.FinalOutcome = WorkflowJson.Serialize(_outcome with { EditedSinceQuotation = true });

        var act = () => Service().SendAsync(Manager, _quotationId, null, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("*Re-price*");
    }

    [Fact]
    public async Task A_quote_that_failed_a_hard_rule_is_never_sent()
    {
        _trip.Status = TripRequestStatus.NeedsOperator;
        _workflow.ValidationResult = WorkflowJson.Serialize(new ProposalValidationResult(false,
            [new ProposalRuleViolation("VEHICLE_SEATS", "Vehicle has 3 seats but pax is 4.", ViolationSeverity.Hard)]));

        var act = () => Service().SendAsync(Manager, _quotationId, null, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("*Hard rule*");
        _trip.Status.Should().Be(TripRequestStatus.NeedsOperator);
    }

    [Fact]
    public async Task A_sent_or_older_version_cannot_be_sent_again()
    {
        _quotations.Setup(q => q.GetAsync(_quotationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QuotationSummary(_quotationId, _trip.Id, 2, "Approved", 1, 1, DateTime.UtcNow));

        var act = () => Service().SendAsync(Manager, _quotationId, null, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("*not sent yet*");
    }

    [Fact]
    public async Task Replan_with_note_passes_the_note_and_the_clients_reason_to_the_planner()
    {
        _trip.Status = TripRequestStatus.ClientDeclined;
        _trip.Cities = TripRequest.JoinCities(["Kandy", "Ella"]);
        _quotations.Setup(q => q.GetDeclineReasonAsync(_quotationId, It.IsAny<CancellationToken>())).ReturnsAsync("Too many temples");
        string? comment = null;
        StartAgentWorkflowRequest? sent = null;
        _agent.Setup(a => a.ReplanAsync(_workflow, It.IsAny<StartAgentWorkflowRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<AgentWorkflow, StartAgentWorkflowRequest, string, CancellationToken>((_, r, c, _) => { sent = r; comment = c; })
            .ReturnsAsync(true);

        var result = await Service().ReplanAsync(Manager, _trip.Id, "Swap a temple for the lake", CancellationToken.None);

        result.TripStatus.Should().Be("Planning");
        _workflow.Status.Should().Be(AgentWorkflowStatus.RevisionRequested);
        comment.Should().Be("Swap a temple for the lake\nThe client declined the last quote: Too many temples");
        sent!.Cities.Should().Equal("Kandy", "Ella");
        sent.LlmProvider.Should().Be("groq");
    }

    [Fact]
    public async Task A_failed_replan_call_moves_the_trip_to_NeedsOperator()
    {
        _trip.Status = TripRequestStatus.ClientDeclined;
        _agent.Setup(a => a.ReplanAsync(_workflow, It.IsAny<StartAgentWorkflowRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<AgentWorkflow, StartAgentWorkflowRequest, string, CancellationToken>((w, _, _, _) => w.Status = AgentWorkflowStatus.FailedSafely)
            .ReturnsAsync(false);

        await Service().ReplanAsync(Manager, _trip.Id, "Cheaper hotels", CancellationToken.None);

        _trip.Status.Should().Be(TripRequestStatus.NeedsOperator);
    }

    [Fact]
    public async Task Replan_is_only_for_a_declined_quote()
    {
        var act = () => Service().ReplanAsync(Manager, _trip.Id, "x", CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public void Previous_violations_are_empty_without_a_stored_validation_result()
    {
        PreviousViolation.FromValidationJson(null).Should().BeEmpty();
        PreviousViolation.FromValidationJson("""{"isValid":true,"violations":[]}""").Should().BeEmpty();
    }
}
