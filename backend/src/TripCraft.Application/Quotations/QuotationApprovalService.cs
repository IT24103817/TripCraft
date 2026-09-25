using Microsoft.Extensions.Logging;
using TripCraft.Application.Common;
using TripCraft.Application.Common.Auditing;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Common.Security;
using TripCraft.Application.Trips;
using TripCraft.Application.Workflows;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Application.Quotations;

/// <summary>
/// The human approval gate (PLAN.md sections 3C, 5 and 6 step 9–10). Only an Operations Manager gets here.
/// Approve runs one transaction: holds -> quotation Approved -> trip Confirmed -> workflow Completed ->
/// approval decision -> audit -> commit. Any failure rolls everything back and returns 409.
/// </summary>
public class QuotationApprovalService(
    IQuotationStore quotations,
    IResourceHoldService holds,
    IAgentWorkflowRepository workflows,
    ITripRequestRepository trips,
    IAgentServiceClient agentService,
    IAuditLogger audit,
    IUnitOfWork unitOfWork,
    ILogger<QuotationApprovalService> logger) : IQuotationApprovalService
{
    private static readonly AgentWorkflowStatus[] Decidable =
        [AgentWorkflowStatus.PendingApproval, AgentWorkflowStatus.RevisionRequested];

    public async Task<QuotationDecisionResponse> ApproveAsync(CurrentUser user, Guid quotationId, string? comment,
        CancellationToken ct)
    {
        var (quotation, trip, workflow) = await LoadAsync(quotationId, ct);
        if (workflow.Status != AgentWorkflowStatus.PendingApproval)
            throw new ConflictException($"Only a PendingApproval workflow can be approved (it is {workflow.Status}).");
        var outcome = WorkflowJson.Deserialize<WorkflowOutcome>(workflow.FinalOutcome)
                      ?? throw new ConflictException("The workflow has no proposal to approve.");
        var holdRequests = BuildHolds(outcome.Proposal, trip);

        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            // 1. Holds, each through Resource Management's overlap check (ConflictException on overlap).
            foreach (var hold in holdRequests)
                await holds.CreateHoldAsync(hold, ct);

            // 2–4. Quotation, trip, workflow.
            await quotations.SetStatusAsync(quotation.Id, QuotationDecision.Approved, ct);
            var tripBefore = trip.Status;
            trip.Status = TripRequestStatus.Confirmed;
            workflow.Status = AgentWorkflowStatus.Completed;
            workflow.CurrentStep = "completed";
            workflow.FinishedAt = DateTime.UtcNow;
            workflow.FinalOutcome = WorkflowJson.Serialize(outcome with
            {
                Decision = new WorkflowDecision("Approved", quotation.Id, user.Id, DateTime.UtcNow, comment, holdRequests)
            });

            // 5–6. Decision and audit.
            quotations.RecordDecision(quotation.Id, user.Id, QuotationDecision.Approved, comment);
            audit.Record(user.Id, "QuotationApproved", "Quotation", quotation.Id,
                new { TripStatus = tripBefore.ToString(), WorkflowStatus = nameof(AgentWorkflowStatus.PendingApproval) },
                new { TripStatus = trip.Status.ToString(), WorkflowStatus = workflow.Status.ToString(), Holds = holdRequests.Count });

            // 7. Commit.
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
            logger.LogError(ex, "Approval of quotation {QuotationId} failed and was rolled back", quotation.Id);
            throw new ConflictException("Approval failed and was rolled back; nothing was saved.");
        }

        return Response(quotation.Id, trip, workflow, "Approved", holdRequests.Count);
    }

    public async Task<QuotationDecisionResponse> RejectAsync(CurrentUser user, Guid quotationId, string? comment,
        CancellationToken ct)
    {
        var (quotation, trip, workflow) = await LoadAsync(quotationId, ct);
        EnsureDecidable(workflow);

        await quotations.SetStatusAsync(quotation.Id, QuotationDecision.Rejected, ct);
        var before = new { TripStatus = trip.Status.ToString(), WorkflowStatus = workflow.Status.ToString() };
        trip.Status = TripRequestStatus.Rejected;
        workflow.Status = AgentWorkflowStatus.Rejected;
        workflow.CurrentStep = "rejected";
        workflow.FinishedAt = DateTime.UtcNow;
        quotations.RecordDecision(quotation.Id, user.Id, QuotationDecision.Rejected, comment);
        audit.Record(user.Id, "QuotationRejected", "Quotation", quotation.Id, before,
            new { TripStatus = trip.Status.ToString(), WorkflowStatus = workflow.Status.ToString(), Comment = comment });

        await unitOfWork.SaveChangesAsync(ct); // one SaveChanges = one transaction
        return Response(quotation.Id, trip, workflow, "Rejected", 0);
    }

    public async Task<QuotationDecisionResponse> RequestRevisionAsync(CurrentUser user, Guid quotationId, string comment,
        CancellationToken ct)
    {
        var (quotation, trip, workflow) = await LoadAsync(quotationId, ct);
        EnsureDecidable(workflow);

        await quotations.SetStatusAsync(quotation.Id, QuotationDecision.RevisionRequested, ct);
        var before = new { TripStatus = trip.Status.ToString(), WorkflowStatus = workflow.Status.ToString() };
        trip.Status = TripRequestStatus.RevisionRequested;
        workflow.Status = AgentWorkflowStatus.RevisionRequested;
        workflow.CurrentStep = "replanning";
        quotations.RecordDecision(quotation.Id, user.Id, QuotationDecision.RevisionRequested, comment);
        audit.Record(user.Id, "QuotationRevisionRequested", "Quotation", quotation.Id, before,
            new { TripStatus = trip.Status.ToString(), WorkflowStatus = workflow.Status.ToString(), Comment = comment });
        await unitOfWork.SaveChangesAsync(ct);

        // After the commit: never hold a DB transaction open during an HTTP call.
        var request = new StartAgentWorkflowRequest(workflow.Id, trip.Id, trip.Objective, trip.StartDate, trip.EndDate,
            trip.Pax, trip.BudgetUsd, trip.Preferences, []);
        if (!await agentService.ReplanAsync(workflow, request, comment, ct))
        {
            // The client already set FailedSafely + error summary; the trip stays RevisionRequested so it can be re-planned.
            audit.Record(user.Id, "AgentWorkflowFailedSafely", nameof(AgentWorkflow), workflow.Id,
                new { Status = nameof(AgentWorkflowStatus.RevisionRequested) },
                new { Status = workflow.Status.ToString(), workflow.ErrorSummary });
            await unitOfWork.SaveChangesAsync(ct);
        }

        return Response(quotation.Id, trip, workflow, "RevisionRequested", 0);
    }

    private async Task<(QuotationSummary, TripRequest, AgentWorkflow)> LoadAsync(Guid quotationId, CancellationToken ct)
    {
        var quotation = await quotations.GetAsync(quotationId, ct)
                        ?? throw new NotFoundException("Quotation not found.");
        if (!quotation.AwaitingDecision)
            throw new ConflictException("This quotation has already been decided.");
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

    private static QuotationDecisionResponse Response(Guid quotationId, TripRequest trip, AgentWorkflow workflow,
        string decision, int holds) =>
        new(quotationId, trip.Id, workflow.Id, decision, trip.Status.ToString(), workflow.Status.ToString(), holds);
}
