using TripCraft.Application.Common;
using TripCraft.Application.Common.Auditing;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Common.Notifications;
using TripCraft.Application.Common.Security;
using TripCraft.Application.Common.Settings;
using TripCraft.Application.Identity;
using TripCraft.Application.Trips;
using TripCraft.Application.Workflows;
using TripCraft.Application.Workflows.Dtos;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Application.Quotations;

public interface IOperatorQuotationService
{
    Task<QuotationDecisionResponse> SendAsync(CurrentUser user, Guid quotationId, string? comment, CancellationToken ct);
    Task<QuotationDecisionResponse> ReplanAsync(CurrentUser user, Guid tripRequestId, string note, CancellationToken ct);
}

/// <summary>
/// The Operations Manager's actions around a quotation once the agents have sent one (v1.1). Quotations normally go
/// to the client without the manager; these are the exceptions:
/// - Send: after "Edit &amp; resend" (trip ClientAccepted) or "Edit &amp; send manually" (NeedsOperator) the re-priced
///   version is sent; the trip goes back to QuotationSent and the tourist must accept the new version.
/// - Replan with note: after the client declined (ClientDeclined → Planning); the Planner gets the note and the client's
///   reason, and its new proposal is sent automatically.
/// The approval gate itself is Confirm (TripConfirmationService).
/// </summary>
public class OperatorQuotationService(
    IQuotationStore quotations,
    IAgentWorkflowRepository workflows,
    ITripRequestRepository trips,
    IAgentServiceClient agentService,
    INotifier notifier,
    TripSettings settings,
    IUserRepository users,
    IEmailDispatcher emails,
    IAuditLogger audit,
    IUnitOfWork unitOfWork) : IOperatorQuotationService
{
    public async Task<QuotationDecisionResponse> SendAsync(CurrentUser user, Guid quotationId, string? comment,
        CancellationToken ct)
    {
        var quotation = await quotations.GetAsync(quotationId, ct) ?? throw new NotFoundException("Quotation not found.");
        if (!quotation.AwaitingDecision)
            throw new ConflictException($"Only a re-priced version that was not sent yet can be sent (this one is {quotation.Status}).");
        var latest = await quotations.GetLatestForTripAsync(quotation.TripRequestId, ct);
        if (latest is not null && latest.Id != quotation.Id)
            throw new ConflictException($"Version {latest.Version} replaced this quotation; send that one.");
        var trip = await trips.GetByIdAsync(quotation.TripRequestId, ct) ?? throw new NotFoundException("Trip request not found.");
        TripStatusMachine.EnsureCanMove(trip.Status, TripRequestStatus.QuotationSent);
        var workflow = await workflows.GetLatestForTripAsync(trip.Id, ct)
                       ?? throw new ConflictException("The trip request has no agent workflow.");
        var outcome = WorkflowJson.Deserialize<WorkflowOutcome>(workflow.FinalOutcome)
                      ?? throw new ConflictException("The workflow has no proposal to send.");
        if (outcome.EditedSinceQuotation)
            throw new ConflictException("The itinerary was edited after this price was made. Re-price it before sending.");
        if (WorkflowJson.Deserialize<ProposalValidationResult>(workflow.ValidationResult) is { HasHard: true })
            throw new ConflictException("This proposal breaks a rule; a quote that failed a Hard rule is never sent.");

        await quotations.SetStatusAsync(quotation.Id, QuotationDecision.Approved, ct);
        quotations.RecordDecision(quotation.Id, user.Id, QuotationDecision.Approved, comment);
        workflow.Status = AgentWorkflowStatus.Approved;
        workflow.CurrentStep = "awaiting-client";
        TripStatusMachine.Move(trip, TripRequestStatus.QuotationSent, user.Id,
            $"Updated quotation v{quotation.Version} sent to the client by the operator.", audit);
        notifier.NotifyTourist(trip, "QuotationUpdated", "Your quote was updated, please review",
            $"Version {quotation.Version}: USD {quotation.TotalUsd:N2}. Please accept or decline it again.");
        if (trip.Tourist is not null && await users.GetByIdAsync(trip.Tourist.UserId, ct) is { } tourist)
            notifier.QueueEmail(tourist.Email, "Your TripCraft quote was updated",
                $"Dear {tourist.FullName},\n\nWe updated your quotation (version {quotation.Version}): USD {quotation.TotalUsd:N2}. " +
                "Please review it in the TripCraft app and accept or decline it.\n\nTripCraft", trip.Id);
        audit.Record(user.Id, "QuotationSent", "Quotation", quotation.Id, null,
            new { TripStatus = trip.Status.ToString(), Comment = comment });

        await unitOfWork.SaveChangesAsync(ct);
        await SendEmailsAsync(ct);
        return QuotationDecisionResponse.From(quotation.Id, trip, workflow, "Sent", 0);
    }

    public async Task<QuotationDecisionResponse> ReplanAsync(CurrentUser user, Guid tripRequestId, string note,
        CancellationToken ct)
    {
        var trip = await trips.GetByIdAsync(tripRequestId, ct) ?? throw new NotFoundException("Trip request not found.");
        if (trip.Status != TripRequestStatus.ClientDeclined)
            throw new ConflictException($"Replan with a note is for a quote the client declined; this trip is {TripStatusMachine.Describe(trip.Status)}.");
        var workflow = await workflows.GetLatestForTripAsync(trip.Id, ct)
                       ?? throw new ConflictException("The trip request has no agent workflow.");
        var declined = await quotations.GetLatestForTripAsync(trip.Id, ct);
        var reason = declined is null ? null : await quotations.GetDeclineReasonAsync(declined.Id, ct);

        var statusBefore = workflow.Status;
        workflow.Status = AgentWorkflowStatus.Planning; // the agents work on it again
        workflow.CurrentStep = "replanning";
        workflow.FinishedAt = null;
        TripStatusMachine.Move(trip, TripRequestStatus.Planning, user.Id, $"Replanning after the client declined: {note.Trim()}", audit);
        await unitOfWork.SaveChangesAsync(ct);

        // After the commit: never hold a DB transaction open during an HTTP call. The Planner gets the operator's
        // note and the client's reason; its new proposal is sent to the client automatically.
        var comment = reason is null ? note.Trim() : $"{note.Trim()}\nThe client declined the last quote: {reason}";
        var request = StartAgentWorkflowRequest.ForReplan(workflow, trip,
            PreviousViolation.FromValidationJson(workflow.ValidationResult), settings.LlmProvider);
        if (!await agentService.ReplanAsync(workflow, request, comment, ct))
        {
            // The client already set the workflow FailedSafely with a summary; the trip waits for the operator.
            TripStatusMachine.Move(trip, TripRequestStatus.NeedsOperator, user.Id,
                workflow.ErrorSummary ?? "The agent service could not re-plan.", audit);
            audit.Record(user.Id, "AgentWorkflowFailedSafely", nameof(AgentWorkflow), workflow.Id,
                new { Status = statusBefore.ToString() },
                new { Status = workflow.Status.ToString(), workflow.ErrorSummary });
            await unitOfWork.SaveChangesAsync(ct);
        }
        return QuotationDecisionResponse.From(declined?.Id ?? Guid.Empty, trip, workflow, "Replan", 0);
    }

    /// <summary>After the commit; the email stays in the outbox (with its error) if sending fails.</summary>
    private async Task SendEmailsAsync(CancellationToken ct)
    {
        try
        {
            await emails.DispatchPendingAsync(ct);
        }
        catch (Exception)
        {
            // Never undo a sent quotation because of email; the outbox row keeps the failure.
        }
    }
}
