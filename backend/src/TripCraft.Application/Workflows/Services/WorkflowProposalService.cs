using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Common;
using TripCraft.Application.Common.Auditing;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Trips;
using TripCraft.Application.Workflows.Dtos;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Application.Workflows.Services;

/// <summary>
/// Receives the agents' final proposal (PLAN.md section 6, step 7–8). Steps:
/// 1. load the workflow and trip; only Planning or RevisionRequested workflows accept a proposal;
/// 2. load the database facts and run the deterministic ProposalValidator;
/// 3. set the status: PendingApproval (valid), RevisionRequested (only Soft), FailedSafely (any Hard);
/// 4. unless FailedSafely, stage a new quotation version;
/// 5. store the validation result and proposal on the workflow, audit, and save in one transaction.
/// Nothing is held here — holds are created only when a manager approves.
/// </summary>
public class WorkflowProposalService(
    IAgentWorkflowRepository workflows,
    ITripRequestRepository trips,
    IAttractionRepository attractions,
    IResourceCatalog resources,
    IQuotationStore quotations,
    ProposalValidator validator,
    IAuditLogger audit,
    IUnitOfWork unitOfWork) : IWorkflowProposalService
{
    private const int MaxErrorSummaryLength = 1000;

    public async Task<ProposalOutcomeResponse> ReceiveAsync(Guid workflowId, AgentProposalRequest proposal, CancellationToken ct)
    {
        // 1. Load.
        var workflow = await workflows.GetByIdAsync(workflowId, ct)
                       ?? throw new NotFoundException("Workflow not found.");
        if (workflow.Status is not (AgentWorkflowStatus.Planning or AgentWorkflowStatus.RevisionRequested))
            throw new ConflictException($"Workflow is {workflow.Status}; it no longer accepts proposals.");
        var trip = await trips.GetByIdAsync(workflow.TripRequestId, ct)
                   ?? throw new NotFoundException("Trip request not found.");
        var previousStatus = workflow.Status;
        var previousTripStatus = trip.Status;

        ProposalValidationResult validation;
        Guid? quotationId = null;

        if (proposal.Status == nameof(AgentWorkflowStatus.FailedSafely))
        {
            // The agents already failed safely (tool error, timeout, bad LLM output).
            var reason = proposal.ErrorSummary ?? "The agent service reported a safe failure.";
            validation = new ProposalValidationResult(false, [new("AGENT_FAILED", reason, ViolationSeverity.Hard)]);
            FailSafely(workflow, trip, $"Agents failed safely: {reason}");
        }
        else
        {
            try
            {
                // 2. Deterministic validation.
                var facts = await LoadFactsAsync(proposal, trip, ct);
                validation = validator.Validate(proposal, trip, facts);

                // 3–4. Status and quotation.
                if (validation.HasHard)
                {
                    var codes = string.Join(", ", validation.Violations.Where(v => v.Severity == ViolationSeverity.Hard)
                        .Select(v => v.Code).Distinct());
                    FailSafely(workflow, trip, $"Deterministic validation failed: {codes}");
                }
                else
                {
                    var onlySoft = validation.HasSoft;
                    workflow.Status = onlySoft ? AgentWorkflowStatus.RevisionRequested : AgentWorkflowStatus.PendingApproval;
                    workflow.CurrentStep = "awaiting-manager";
                    trip.Status = onlySoft ? TripRequestStatus.RevisionRequested : TripRequestStatus.PendingApproval;
                    quotationId = await quotations.AddVersionAsync(ToDraft(trip, workflow, proposal.Quotation!, facts), ct);
                }
            }
            catch (ComponentNotAvailableException ex)
            {
                validation = new ProposalValidationResult(false, [new("COMPONENT_UNAVAILABLE", ex.Message, ViolationSeverity.Hard)]);
                FailSafely(workflow, trip, ex.Message);
            }
        }

        // 5. Store and commit.
        if (proposal.Plan is { ValueKind: System.Text.Json.JsonValueKind.Object } plan)
            workflow.Plan = plan.GetRawText();
        workflow.ValidationResult = WorkflowJson.Serialize(validation);
        workflow.FinalOutcome = WorkflowJson.Serialize(new WorkflowOutcome(
            new StoredProposal(proposal.Days, proposal.Resources, proposal.Quotation, proposal.Violations,
                proposal.Replans, quotationId),
            Decision: null));

        audit.Record(null, "AgentProposalReceived", nameof(AgentWorkflow), workflow.Id,
            new { Status = previousStatus.ToString() },
            new
            {
                Status = workflow.Status.ToString(),
                Violations = validation.Violations.Select(v => v.Code).ToList(),
                QuotationId = quotationId,
                proposal.Replans
            });
        if (trip.Status != previousTripStatus)
            audit.Record(null, "TripRequestStatusChanged", nameof(TripRequest), trip.Id,
                new { Status = previousTripStatus.ToString() }, new { Status = trip.Status.ToString() });
        await unitOfWork.SaveChangesAsync(ct);

        return new ProposalOutcomeResponse(workflow.Id, workflow.Status.ToString(), quotationId, validation);
    }

    private static void FailSafely(AgentWorkflow workflow, TripRequest trip, string reason)
    {
        workflow.Status = AgentWorkflowStatus.FailedSafely;
        workflow.ErrorSummary = reason.Length > MaxErrorSummaryLength ? reason[..MaxErrorSummaryLength] : reason;
        workflow.FinishedAt = DateTime.UtcNow;
        workflow.CurrentStep = "failed";
        // A first run puts the trip back to Submitted so it can be retried; a re-plan leaves RevisionRequested.
        if (trip.Status == TripRequestStatus.Planning)
            trip.Status = TripRequestStatus.Submitted;
    }

    /// <summary>Loads only the rows the proposal refers to. Unparseable ids are left for the validator to flag.</summary>
    private async Task<ProposalFacts> LoadFactsAsync(AgentProposalRequest proposal, TripRequest trip, CancellationToken ct)
    {
        var attractionIds = (proposal.Days ?? [])
            .SelectMany(d => d.Stops ?? [])
            .Select(s => ProposalValidator.ParseId(s.AttractionId))
            .OfType<Guid>()
            .Distinct()
            .ToList();
        var entryFees = await attractions.QueryActive()
            .Where(a => attractionIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, a => a.EntryFeeLkr, ct);

        var guideId = ProposalValidator.ParseId(proposal.Resources?.GuideId);
        var vehicleId = ProposalValidator.ParseId(proposal.Resources?.VehicleId);
        var guide = guideId is null ? null : await resources.GetGuideAsync(guideId.Value, ct);
        var vehicle = vehicleId is null ? null : await resources.GetVehicleAsync(vehicleId.Value, ct);

        var roomTypes = new Dictionary<Guid, RoomOption>();
        var roomTypeIds = (proposal.Resources?.Rooms ?? [])
            .Select(r => ProposalValidator.ParseId(r.RoomTypeId)).OfType<Guid>().Distinct();
        foreach (var id in roomTypeIds)
        {
            if (await resources.GetRoomTypeAsync(id, ct) is { } roomType)
                roomTypes[id] = roomType;
        }

        var guideOverlaps = guide is not null && await resources.HasOverlappingHoldAsync(
            ResourceType.Guide, guide.Id, trip.StartDate, trip.EndDate, ct);
        var vehicleOverlaps = vehicle is not null && await resources.HasOverlappingHoldAsync(
            ResourceType.Vehicle, vehicle.Id, trip.StartDate, trip.EndDate, ct);

        return new ProposalFacts(entryFees, guide, vehicle, roomTypes, guideOverlaps, vehicleOverlaps,
            await resources.GetRateCardAsync(ct));
    }

    private static QuotationDraft ToDraft(TripRequest trip, AgentWorkflow workflow, ProposalQuotation q, ProposalFacts facts) => new(
        trip.Id, workflow.Id, q.SubtotalLkr, q.MarginPct, q.TotalLkr, q.TotalUsd, q.FxRate, q.FxAsOf, q.FxStale,
        (q.Lines ?? []).Select(l => new QuotationDraftLine(
                l.LineType, QuotationLineNames.Describe(l, facts), l.Qty, l.UnitLkr, l.AmountLkr))
            .ToList());
}
