using TripCraft.Application.Common;
using TripCraft.Application.Common.Auditing;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Common.Notifications;
using TripCraft.Application.Common.Security;
using TripCraft.Application.Trips;
using TripCraft.Application.Workflows;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Application.Quotations;

public interface IQuotationClientService
{
    Task<QuotationDecisionResponse> AcceptAsync(CurrentUser user, Guid quotationId, CancellationToken ct);
    Task<QuotationDecisionResponse> DeclineAsync(CurrentUser user, Guid quotationId, string reason, CancellationToken ct);
}

/// <summary>
/// The tourist's answer to a quotation that was sent (trip QuotationSent, v1.1):
/// Accept → ClientAccepted and the managers are told to confirm;
/// Decline (reason required) → back to PendingReview, and the reason is shown to the manager.
/// </summary>
public class QuotationClientService(
    IQuotationStore quotations,
    IAgentWorkflowRepository workflows,
    ITripRequestRepository trips,
    INotifier notifier,
    IAuditLogger audit,
    IUnitOfWork unitOfWork) : IQuotationClientService
{
    public async Task<QuotationDecisionResponse> AcceptAsync(CurrentUser user, Guid quotationId, CancellationToken ct)
    {
        var (quotation, trip, workflow) = await LoadAsync(user, quotationId, ct);
        TripStatusMachine.EnsureCanMove(trip.Status, TripRequestStatus.ClientAccepted);

        await quotations.SetStatusAsync(quotation.Id, QuotationDecision.Accepted, ct);
        quotations.RecordDecision(quotation.Id, user.Id, QuotationDecision.Accepted, null);
        workflow.CurrentStep = "awaiting-confirmation";
        TripStatusMachine.Move(trip, TripRequestStatus.ClientAccepted, user.Id,
            $"The client accepted quotation v{quotation.Version}.", audit);
        await notifier.NotifyManagersAsync("ClientAccepted", "Client accepted a quotation",
            $"Version {quotation.Version} (USD {quotation.TotalUsd:N2}) was accepted. Confirm the trip to book it.", trip.Id, ct);
        audit.Record(user.Id, "QuotationAccepted", "Quotation", quotation.Id, null, new { quotation.TotalUsd });

        await unitOfWork.SaveChangesAsync(ct);
        return QuotationApprovalService.Response(quotation.Id, trip, workflow, "Accepted", 0);
    }

    public async Task<QuotationDecisionResponse> DeclineAsync(CurrentUser user, Guid quotationId, string reason,
        CancellationToken ct)
    {
        var (quotation, trip, workflow) = await LoadAsync(user, quotationId, ct);
        TripStatusMachine.EnsureCanMove(trip.Status, TripRequestStatus.PendingReview);

        await quotations.SetStatusAsync(quotation.Id, QuotationDecision.Declined, ct);
        quotations.RecordDecision(quotation.Id, user.Id, QuotationDecision.Declined, reason);
        // Back on the manager's desk: request a revision, or edit and re-price, then send again.
        workflow.Status = AgentWorkflowStatus.PendingApproval;
        workflow.CurrentStep = "awaiting-manager";
        TripStatusMachine.Move(trip, TripRequestStatus.PendingReview, user.Id, $"Declined by the client: {reason.Trim()}", audit);
        await notifier.NotifyManagersAsync("ClientDeclined", "Client declined a quotation",
            $"Version {quotation.Version} was declined: {reason.Trim()}", trip.Id, ct);
        audit.Record(user.Id, "QuotationDeclined", "Quotation", quotation.Id, null, new { Reason = reason.Trim() });

        await unitOfWork.SaveChangesAsync(ct);
        return QuotationApprovalService.Response(quotation.Id, trip, workflow, "Declined", 0);
    }

    /// <summary>Only the trip's tourist, only the newest version, only once it was sent and not yet answered.</summary>
    private async Task<(QuotationSummary, TripRequest, AgentWorkflow)> LoadAsync(CurrentUser user, Guid quotationId,
        CancellationToken ct)
    {
        var quotation = await quotations.GetAsync(quotationId, ct) ?? throw new NotFoundException("Quotation not found.");
        var trip = await trips.GetByIdAsync(quotation.TripRequestId, ct) ?? throw new NotFoundException("Trip request not found.");
        TripAccess.EnsureCanAccess(user, trip.Tourist?.UserId);
        if (quotation.Status != "Approved" || quotation.AcceptedAt is not null)
            throw new ConflictException("Only a quotation that was sent to you and not answered yet can be accepted or declined.");
        var latest = await quotations.GetLatestForTripAsync(trip.Id, ct);
        if (latest is not null && latest.Id != quotation.Id)
            throw new ConflictException($"Version {latest.Version} replaced this quotation.");
        var workflow = await workflows.GetLatestForTripAsync(trip.Id, ct)
                       ?? throw new ConflictException("The trip request has no agent workflow.");
        return (quotation, trip, workflow);
    }
}
