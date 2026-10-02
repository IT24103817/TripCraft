using Microsoft.Extensions.Logging;
using TripCraft.Application.Common;
using TripCraft.Application.Common.Auditing;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Common.Notifications;
using TripCraft.Application.Common.Security;
using TripCraft.Application.Identity;
using TripCraft.Application.Trips;
using TripCraft.Application.Vouchers;
using TripCraft.Application.Workflows;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Application.Quotations;

public interface ITripConfirmationService
{
    Task<QuotationDecisionResponse> ConfirmAsync(CurrentUser user, Guid tripRequestId, CancellationToken ct);
}

/// <summary>
/// Confirm (manager, trip ClientAccepted) is the single booking transaction (PLAN.md section 6 step 10–11, v1.1):
/// holds → saved itinerary → vouchers → decision → trip Confirmed → workflow Completed → notifications → outbox
/// email → audit → commit. Any failure rolls everything back and returns 409; the trip stays ClientAccepted.
/// The email is sent after the commit, so a mail problem never undoes a booking. This is the human approval gate
/// (Operations Manager only): quotations reach the client without a manager, but nothing is held before Confirm.
/// </summary>
public class TripConfirmationService(
    IQuotationStore quotations,
    IResourceHoldService holds,
    IAgentWorkflowRepository workflows,
    ITripRequestRepository trips,
    IVoucherRepository vouchers,
    VoucherSigner signer,
    IUserRepository users,
    INotifier notifier,
    IEmailDispatcher emails,
    IAuditLogger audit,
    IUnitOfWork unitOfWork,
    ILogger<TripConfirmationService> logger) : ITripConfirmationService
{
    public async Task<QuotationDecisionResponse> ConfirmAsync(CurrentUser user, Guid tripRequestId, CancellationToken ct)
    {
        var trip = await trips.GetByIdAsync(tripRequestId, ct) ?? throw new NotFoundException("Trip request not found.");
        TripStatusMachine.EnsureCanMove(trip.Status, TripRequestStatus.Confirmed);
        var quotation = await quotations.GetLatestForTripAsync(trip.Id, ct);
        if (quotation is not { Status: "Approved", AcceptedAt: not null })
            throw new ConflictException("The client has not accepted the newest quotation version; Confirm is available once they do.");
        var workflow = await workflows.GetLatestForTripAsync(trip.Id, ct)
                       ?? throw new ConflictException("The trip request has no agent workflow.");
        var outcome = WorkflowJson.Deserialize<WorkflowOutcome>(workflow.FinalOutcome)
                      ?? throw new ConflictException("The workflow has no proposal to confirm.");
        // The holds are built from the proposal: it must still be the one the client accepted.
        if (outcome.EditedSinceQuotation)
            throw new ConflictException("The trip was edited after the client accepted. Re-price and resend it; the client must accept again.");
        var holdRequests = BuildHolds(outcome.Proposal, trip);
        var tripVouchers = VoucherBuilder.Build(trip, outcome.Proposal, signer);
        var tourist = trip.Tourist is null ? null : await users.GetByIdAsync(trip.Tourist.UserId, ct);

        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            // 1. Holds, each through Resource Management's overlap check (ConflictException on overlap).
            foreach (var hold in holdRequests)
                await holds.CreateHoldAsync(hold, ct);

            // 2. The accepted days become the trip's saved itinerary (Component A tables).
            if (await trips.HasItineraryAsync(trip.Id, ct))
                throw new ConflictException("This trip request already has a saved itinerary.");
            trips.AddItinerary(ApprovedItinerary.Build(trip.Id, outcome.Proposal));

            // 3. Signed vouchers: one for the trip, one per hotel night.
            foreach (var voucher in tripVouchers)
                vouchers.Add(voucher);

            // 4. Decision, workflow, trip.
            await quotations.SetStatusAsync(quotation.Id, QuotationDecision.Confirmed, ct);
            quotations.RecordDecision(quotation.Id, user.Id, QuotationDecision.Confirmed, null);
            workflow.Status = AgentWorkflowStatus.Completed;
            workflow.CurrentStep = "completed";
            workflow.FinishedAt = DateTime.UtcNow;
            workflow.FinalOutcome = WorkflowJson.Serialize(outcome with
            {
                Decision = new WorkflowDecision("Confirmed", quotation.Id, user.Id, DateTime.UtcNow, null, holdRequests)
            });
            TripStatusMachine.Move(trip, TripRequestStatus.Confirmed, user.Id,
                $"Confirmed: {holdRequests.Count} holds placed and {tripVouchers.Count} vouchers issued.", audit);

            // 5. Tell the tourist (app + email) and the guide.
            notifier.NotifyTourist(trip, "TripConfirmed", "Your trip is confirmed",
                "Your vouchers are ready in the app. Show the trip voucher to your guide on day 1.");
            if (ProposalValidator.ParseId(outcome.Proposal.Resources?.GuideId) is { } guideId)
                await notifier.NotifyGuideAsync(guideId, "TripAssigned", "New trip assigned",
                    $"{trip.StartDate:dd MMM} – {trip.EndDate:dd MMM}, {trip.Pax} people. See your schedule.", trip.Id, ct);
            if (tourist is not null)
                notifier.QueueEmail(tourist.Email, "Your TripCraft trip is confirmed",
                    $"Dear {tourist.FullName},\n\nYour trip from {trip.StartDate:dd MMM yyyy} to {trip.EndDate:dd MMM yyyy} is " +
                    $"confirmed (USD {quotation.TotalUsd:N2}). Your vouchers are in the TripCraft app.\n\nTripCraft", trip.Id);

            // 6. Audit and commit.
            audit.Record(user.Id, "TripConfirmed", nameof(TripRequest), trip.Id, null,
                new { QuotationId = quotation.Id, Holds = holdRequests.Count, Vouchers = tripVouchers.Count });
            await unitOfWork.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (Exception ex) when (ex is ConflictException or ComponentNotAvailableException)
        {
            unitOfWork.DiscardChanges();
            throw;
        }
        catch (Exception ex)
        {
            unitOfWork.DiscardChanges();
            logger.LogError(ex, "Confirmation of trip {TripRequestId} failed and was rolled back", trip.Id);
            throw new ConflictException("Confirmation failed and was rolled back; nothing was saved.");
        }

        await SendEmailsAsync(ct);
        return QuotationDecisionResponse.From(quotation.Id, trip, workflow, "Confirmed", holdRequests.Count);
    }

    private async Task SendEmailsAsync(CancellationToken ct)
    {
        try
        {
            await emails.DispatchPendingAsync(ct);
        }
        catch (Exception ex)
        {
            // The booking is committed; the email stays in the outbox.
            logger.LogWarning(ex, "Outbox dispatch failed after a confirmation");
        }
    }

    /// <summary>
    /// One hold for the guide and one for the vehicle over the whole trip, and one per room type and night
    /// with the number of rooms as quantity.
    /// </summary>
    public static List<ResourceHoldRequest> BuildHolds(StoredProposal proposal, TripRequest trip)
    {
        var guideId = ProposalValidator.ParseId(proposal.Resources?.GuideId)
                      ?? throw new ConflictException("The proposal has no valid guide.");
        var vehicleId = ProposalValidator.ParseId(proposal.Resources?.VehicleId)
                        ?? throw new ConflictException("The proposal has no valid vehicle.");

        var result = new List<ResourceHoldRequest>
        {
            new(ResourceType.Guide, guideId, trip.Id, trip.StartDate, trip.EndDate, 1),
            new(ResourceType.Vehicle, vehicleId, trip.Id, trip.StartDate, trip.EndDate, 1)
        };
        foreach (var group in (proposal.Resources?.Rooms ?? []).GroupBy(r => (r.RoomTypeId, r.Night)).OrderBy(g => g.Key.Night))
        {
            var roomTypeId = ProposalValidator.ParseId(group.Key.RoomTypeId)
                             ?? throw new ConflictException($"Room type '{group.Key.RoomTypeId}' is not valid.");
            result.Add(new ResourceHoldRequest(ResourceType.Room, roomTypeId, trip.Id, group.Key.Night, group.Key.Night,
                group.Count()));
        }
        return result;
    }
}
