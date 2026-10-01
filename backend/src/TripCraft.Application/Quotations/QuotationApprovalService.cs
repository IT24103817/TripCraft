using TripCraft.Application.Common;
using TripCraft.Application.Common.Auditing;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Common.Notifications;
using TripCraft.Application.Common.Security;
using TripCraft.Application.Trips;
using TripCraft.Application.Workflows;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Application.Quotations;

/// <summary>
/// The manager's review gate at PendingReview (PLAN.md section 6 step 9–10, v1.1 lifecycle). Only an Operations
/// Manager gets here. Nothing is held here: holds are placed only by Confirm, after the client accepted.
/// - Approve = "Send to client": PendingReview → QuotationSent, the tourist is notified.
/// - Reject: PendingReview → Cancelled (the operator turns the trip down).
/// - Request revision: PendingReview → RevisionRequested, the Planner re-plans with the manager's comment and the
///   next proposal brings the trip back to PendingReview with a new quotation version.
/// </summary>
public class QuotationApprovalService(
    IQuotationStore quotations,
    IAgentWorkflowRepository workflows,
    ITripRequestRepository trips,
    IAgentServiceClient agentService,
    INotifier notifier,
    IAuditLogger audit,
    IUnitOfWork unitOfWork) : IQuotationApprovalService
{
    private static readonly AgentWorkflowStatus[] Decidable =
        [AgentWorkflowStatus.PendingApproval, AgentWorkflowStatus.RevisionRequested];

    public async Task<QuotationDecisionResponse> ApproveAsync(CurrentUser user, Guid quotationId, string? comment,
        CancellationToken ct)
    {
        var (quotation, trip, workflow) = await LoadAsync(quotationId, allowDeclined: false, ct);
        TripStatusMachine.EnsureCanMove(trip.Status, TripRequestStatus.QuotationSent);
        if (workflow.Status != AgentWorkflowStatus.PendingApproval)
            throw new ConflictException(
                "This proposal breaks a rule (for example it is over budget). Request a revision, or edit and re-price it first.");
        var outcome = WorkflowJson.Deserialize<WorkflowOutcome>(workflow.FinalOutcome)
                      ?? throw new ConflictException("The workflow has no proposal to send.");
        if (outcome.EditedSinceQuotation)
            throw new ConflictException("The itinerary was edited after this price was made. Re-price it before sending.");

        await quotations.SetStatusAsync(quotation.Id, QuotationDecision.Approved, ct);
        quotations.RecordDecision(quotation.Id, user.Id, QuotationDecision.Approved, comment);
        workflow.Status = AgentWorkflowStatus.Approved;
        workflow.CurrentStep = "awaiting-client";
        workflow.FinalOutcome = WorkflowJson.Serialize(outcome with
        {
            Decision = new WorkflowDecision("Approved", quotation.Id, user.Id, DateTime.UtcNow, comment, [])
        });
        TripStatusMachine.Move(trip, TripRequestStatus.QuotationSent, user.Id,
            $"Quotation v{quotation.Version} sent to the client.", audit);
        notifier.NotifyTourist(trip, "QuotationSent", "Your quotation is ready",
            $"Version {quotation.Version}: USD {quotation.TotalUsd:N2}. Open the trip to accept or decline it.");
        audit.Record(user.Id, "QuotationApproved", "Quotation", quotation.Id, null,
            new { TripStatus = trip.Status.ToString(), WorkflowStatus = workflow.Status.ToString(), Comment = comment });

        await unitOfWork.SaveChangesAsync(ct); // one SaveChanges = one transaction
        return Response(quotation.Id, trip, workflow, "Approved", 0);
    }

    public async Task<QuotationDecisionResponse> RejectAsync(CurrentUser user, Guid quotationId, string? comment,
        CancellationToken ct)
    {
        var (quotation, trip, workflow) = await LoadAsync(quotationId, allowDeclined: true, ct);
        EnsureDecidable(workflow);
        TripStatusMachine.EnsureCanMove(trip.Status, TripRequestStatus.Cancelled);

        await quotations.SetStatusAsync(quotation.Id, QuotationDecision.Rejected, ct);
        quotations.RecordDecision(quotation.Id, user.Id, QuotationDecision.Rejected, comment);
        workflow.Status = AgentWorkflowStatus.Rejected;
        workflow.CurrentStep = "rejected";
        workflow.FinishedAt = DateTime.UtcNow;
        TripStatusMachine.Move(trip, TripRequestStatus.Cancelled, user.Id,
            string.IsNullOrWhiteSpace(comment) ? "Rejected by the operator." : $"Rejected by the operator: {comment.Trim()}", audit);
        notifier.NotifyTourist(trip, "TripRejected", "We cannot offer this trip",
            string.IsNullOrWhiteSpace(comment) ? "The operator could not offer this trip." : comment.Trim());
        audit.Record(user.Id, "QuotationRejected", "Quotation", quotation.Id, null,
            new { TripStatus = trip.Status.ToString(), WorkflowStatus = workflow.Status.ToString(), Comment = comment });

        await unitOfWork.SaveChangesAsync(ct);
        return Response(quotation.Id, trip, workflow, "Rejected", 0);
    }

    public async Task<QuotationDecisionResponse> RequestRevisionAsync(CurrentUser user, Guid quotationId, string comment,
        CancellationToken ct)
    {
        var (quotation, trip, workflow) = await LoadAsync(quotationId, allowDeclined: true, ct);
        EnsureDecidable(workflow);

        await quotations.SetStatusAsync(quotation.Id, QuotationDecision.RevisionRequested, ct);
        quotations.RecordDecision(quotation.Id, user.Id, QuotationDecision.RevisionRequested, comment);
        workflow.Status = AgentWorkflowStatus.RevisionRequested;
        workflow.CurrentStep = "replanning";
        TripStatusMachine.Move(trip, TripRequestStatus.RevisionRequested, user.Id, $"Revision requested: {comment.Trim()}", audit);
        audit.Record(user.Id, "QuotationRevisionRequested", "Quotation", quotation.Id, null,
            new { TripStatus = trip.Status.ToString(), WorkflowStatus = workflow.Status.ToString(), Comment = comment });
        await unitOfWork.SaveChangesAsync(ct);

        // After the commit: never hold a DB transaction open during an HTTP call.
        // The rejected proposal's violations go with the replan, so e.g. OVER_BUDGET makes the Planner pick budget hotels.
        var request = StartAgentWorkflowRequest.ForReplan(workflow, trip,
            PreviousViolation.FromValidationJson(workflow.ValidationResult));
        if (!await agentService.ReplanAsync(workflow, request, comment, ct))
        {
            // The client already set the workflow FailedSafely with a summary; the trip follows so it can be retried.
            TripStatusMachine.Move(trip, TripRequestStatus.FailedSafely, user.Id,
                workflow.ErrorSummary ?? "The agent service could not re-plan.", audit);
            audit.Record(user.Id, "AgentWorkflowFailedSafely", nameof(AgentWorkflow), workflow.Id,
                new { Status = nameof(AgentWorkflowStatus.RevisionRequested) },
                new { Status = workflow.Status.ToString(), workflow.ErrorSummary });
            await unitOfWork.SaveChangesAsync(ct);
        }

        return Response(quotation.Id, trip, workflow, "RevisionRequested", 0);
    }

    /// <summary>
    /// The quotation must be the trip's newest version and still Pending. Reject and Request revision also accept a
    /// version the client declined (the trip is back in review with the client's reason).
    /// </summary>
    private async Task<(QuotationSummary, TripRequest, AgentWorkflow)> LoadAsync(Guid quotationId, bool allowDeclined,
        CancellationToken ct)
    {
        var quotation = await quotations.GetAsync(quotationId, ct)
                        ?? throw new NotFoundException("Quotation not found.");
        if (!quotation.AwaitingDecision && !(allowDeclined && quotation.Status == "Declined"))
            throw new ConflictException($"This quotation is {quotation.Status}; it has already been decided.");
        var latest = await quotations.GetLatestForTripAsync(quotation.TripRequestId, ct);
        if (latest is not null && latest.Id != quotation.Id)
            throw new ConflictException($"Version {latest.Version} replaced this quotation; decide on that one.");
        var trip = await trips.GetByIdAsync(quotation.TripRequestId, ct)
                   ?? throw new NotFoundException("Trip request not found.");
        var workflow = await workflows.GetLatestForTripAsync(trip.Id, ct)
                       ?? throw new ConflictException("The trip request has no agent workflow.");
        return (quotation, trip, workflow);
    }

    private static void EnsureDecidable(AgentWorkflow workflow)
    {
        if (!Decidable.Contains(workflow.Status))
            throw new ConflictException($"The workflow is {workflow.Status}; no decision can be made.");
    }

    public static QuotationDecisionResponse Response(Guid quotationId, TripRequest trip, AgentWorkflow workflow,
        string decision, int holds) =>
        new(quotationId, trip.Id, workflow.Id, decision, trip.Status.ToString(), workflow.Status.ToString(), holds);
}
