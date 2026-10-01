using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TripCraft.Application.Common;
using TripCraft.Application.Common.Auditing;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Common.Notifications;
using TripCraft.Application.Common.Security;
using TripCraft.Application.Identity;
using TripCraft.Application.Quotations;
using TripCraft.Application.Trips;
using TripCraft.Application.Vouchers;
using TripCraft.Application.Workflows;
using TripCraft.Application.Workflows.Ports;
using TripCraft.Tests.Workflows;

namespace TripCraft.Tests.Quotations;

/// <summary>Unit tests of Confirm, the single booking transaction (v1.1), with every dependency mocked.</summary>
public class TripConfirmationServiceTests
{
    public const string SigningKey = "unit-test-voucher-signing-key-0123456789";
    private static readonly CurrentUser Manager = new(Guid.NewGuid(), UserRole.OperationsManager);
    private static readonly DateOnly Start = new(2026, 10, 10);

    private readonly Mock<IQuotationStore> _quotations = new();
    private readonly Mock<IResourceHoldService> _holds = new();
    private readonly Mock<IAgentWorkflowRepository> _workflows = new();
    private readonly Mock<ITripRequestRepository> _trips = new();
    private readonly Mock<IVoucherRepository> _vouchers = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<INotifier> _notifier = new();
    private readonly Mock<IEmailDispatcher> _emails = new();
    private readonly Mock<IAuditLogger> _audit = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly TripRequest _trip = new() { StartDate = Start, EndDate = Start.AddDays(4), Pax = 4, Status = TripRequestStatus.ClientAccepted };
    private readonly AgentWorkflow _workflow = new() { Status = AgentWorkflowStatus.Approved };
    private readonly Guid _quotationId = Guid.NewGuid();

    public TripConfirmationServiceTests()
    {
        var proposal = TestProposals.Golden(Start, DemoAttractions.Random);
        _workflow.TripRequestId = _trip.Id;
        _workflow.FinalOutcome = WorkflowJson.Serialize(new WorkflowOutcome(
            new StoredProposal(proposal.Days, proposal.Resources, proposal.Quotation, [], 0, _quotationId), null));
        _quotations.Setup(q => q.GetLatestForTripAsync(_trip.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QuotationSummary(_quotationId, _trip.Id, 1, "Approved", 187220, 624.07m, DateTime.UtcNow));
        _trips.Setup(t => t.GetByIdAsync(_trip.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_trip);
        _workflows.Setup(w => w.GetLatestForTripAsync(_trip.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_workflow);
        _unitOfWork.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<IUnitOfWorkTransaction>());
    }

    private TripConfirmationService Service() => new(_quotations.Object, _holds.Object, _workflows.Object, _trips.Object,
        _vouchers.Object, new VoucherSigner(SigningKey), _users.Object, _notifier.Object, _emails.Object, _audit.Object,
        _unitOfWork.Object, NullLogger<TripConfirmationService>.Instance);

    [Fact]
    public void Holds_are_one_per_guide_and_vehicle_and_one_per_room_type_and_night()
    {
        var outcome = WorkflowJson.Deserialize<WorkflowOutcome>(_workflow.FinalOutcome)!;

        var holds = TripConfirmationService.BuildHolds(outcome.Proposal, _trip);

        holds.Should().HaveCount(6);
        holds.Where(h => h.Type != ResourceType.Room).Should().OnlyContain(h => h.From == Start && h.To == Start.AddDays(4));
        holds.Where(h => h.Type == ResourceType.Room).Select(h => (h.From, h.Quantity))
            .Should().Equal((Start, 2), (Start.AddDays(1), 2), (Start.AddDays(2), 2), (Start.AddDays(3), 2));
    }

    [Fact]
    public async Task Confirm_holds_issues_vouchers_commits_once_then_sends_the_email()
    {
        var order = new List<string>();
        _holds.Setup(h => h.CreateHoldAsync(It.IsAny<ResourceHoldRequest>(), It.IsAny<CancellationToken>()))
            .Callback(() => order.Add("hold")).Returns(Task.CompletedTask);
        _vouchers.Setup(v => v.Add(It.IsAny<Voucher>())).Callback(() => order.Add("voucher"));
        _quotations.Setup(q => q.RecordDecision(_quotationId, Manager.Id, QuotationDecision.Confirmed, null)).Callback(() => order.Add("decision"));
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Callback(() => order.Add("save")).Returns(Task.CompletedTask);
        _emails.Setup(e => e.DispatchPendingAsync(It.IsAny<CancellationToken>())).Callback(() => order.Add("email")).Returns(Task.CompletedTask);

        var result = await Service().ConfirmAsync(Manager, _trip.Id, CancellationToken.None);

        result.HoldsCreated.Should().Be(6);
        _trip.Status.Should().Be(TripRequestStatus.Confirmed);
        _workflow.Status.Should().Be(AgentWorkflowStatus.Completed);
        // 6 holds, 1 trip voucher + 4 hotel nights, the decision, one commit, then the email after the commit.
        order.Should().Equal("hold", "hold", "hold", "hold", "hold", "hold",
            "voucher", "voucher", "voucher", "voucher", "voucher", "decision", "save", "email");
        _trips.Verify(t => t.AddItinerary(It.IsAny<Itinerary>()), Times.Once);
        _notifier.Verify(n => n.NotifyTourist(_trip, "TripConfirmed", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task A_hold_conflict_discards_everything_and_the_trip_stays_ClientAccepted()
    {
        _holds.SetupSequence(h => h.CreateHoldAsync(It.IsAny<ResourceHoldRequest>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .ThrowsAsync(new ConflictException("Vehicle already held."));

        var act = () => Service().ConfirmAsync(Manager, _trip.Id, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("Vehicle already held.");
        _trip.Status.Should().Be(TripRequestStatus.ClientAccepted);
        _unitOfWork.Verify(u => u.DiscardChanges(), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _emails.Verify(e => e.DispatchPendingAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Any_other_failure_is_rolled_back_and_reported_as_409()
    {
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new TimeoutException("db timeout"));

        var act = () => Service().ConfirmAsync(Manager, _trip.Id, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("*rolled back*");
        _unitOfWork.Verify(u => u.DiscardChanges(), Times.Once);
    }

    [Fact]
    public async Task A_failing_email_never_undoes_the_confirmation()
    {
        _emails.Setup(e => e.DispatchPendingAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("smtp down"));

        var result = await Service().ConfirmAsync(Manager, _trip.Id, CancellationToken.None);

        result.TripStatus.Should().Be("Confirmed");
    }

    [Theory]
    [InlineData(TripRequestStatus.QuotationSent)]
    [InlineData(TripRequestStatus.PendingReview)]
    [InlineData(TripRequestStatus.Confirmed)]
    public async Task Confirm_is_only_allowed_after_the_client_accepted(TripRequestStatus status)
    {
        _trip.Status = status;

        var act = () => Service().ConfirmAsync(Manager, _trip.Id, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        _holds.Verify(h => h.CreateHoldAsync(It.IsAny<ResourceHoldRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Confirm_needs_the_newest_quotation_to_be_accepted()
    {
        _quotations.Setup(q => q.GetLatestForTripAsync(_trip.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QuotationSummary(_quotationId, _trip.Id, 1, "Approved", 1, 1, null));

        var act = () => Service().ConfirmAsync(Manager, _trip.Id, CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("*not accepted*");
    }
}
