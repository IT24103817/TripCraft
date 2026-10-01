using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Common;
using TripCraft.Application.Common.Auditing;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Common.Notifications;
using TripCraft.Application.Trips;
using TripCraft.Application.Workflows.Dtos;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Application.Workflows.Services;

/// <summary>
/// Receives the agents' final proposal (PLAN.md section 6, step 7–8). Steps:
/// 1. load the workflow and trip; only Planning or RevisionRequested workflows accept a proposal;
/// 2. load the database facts and run the deterministic ProposalValidator;
/// 3. workflow status: PendingApproval (valid), RevisionRequested (only Soft), FailedSafely (any Hard);
///    trip status (v1.1, through TripStatusMachine): PendingReview, or FailedSafely;
/// 4. unless FailedSafely, stage a new quotation version with a snapshot of what it priced;
/// 5. store the validation result and proposal on the workflow, notify the managers, audit, save in one transaction.
/// Nothing is held here — holds are created only when the manager confirms a trip the client accepted.
/// </summary>
public class WorkflowProposalService(
    IAgentWorkflowRepository workflows,
    ITripRequestRepository trips,
    ProposalFactsLoader factsLoader,
    IQuotationStore quotations,
    ProposalValidator validator,
    INotifier notifier,
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

        ProposalValidationResult validation;
        Guid? quotationId = null;
        var stored = new StoredProposal(proposal.Days, proposal.Resources, proposal.Quotation, proposal.Violations,
            proposal.Replans, null);

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
                var facts = await factsLoader.LoadAsync(proposal, trip, ct);
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
                    workflow.Status = validation.HasSoft ? AgentWorkflowStatus.RevisionRequested : AgentWorkflowStatus.PendingApproval;
                    workflow.CurrentStep = "awaiting-manager";
                    quotationId = await quotations.AddVersionAsync(
                        ToDraft(trip, workflow, proposal.Quotation!, facts, ProposalSnapshot.Serialize(stored)), ct);
                    TripStatusMachine.Move(trip, TripRequestStatus.PendingReview, null,
                        validation.HasSoft ? "Proposal ready for review with a warning (over budget)." : "Proposal ready for review.",
                        audit);
                    await notifier.NotifyManagersAsync("ReviewNeeded", "Trip ready for review",
                        $"A new proposal for \"{Shorten(trip.Objective)}\" is waiting for your review.", trip.Id, ct);
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
        workflow.FinalOutcome = WorkflowJson.Serialize(new WorkflowOutcome(stored with { QuotationId = quotationId }, null));

        audit.Record(null, "AgentProposalReceived", nameof(AgentWorkflow), workflow.Id,
            new { Status = previousStatus.ToString() },
            new
            {
                Status = workflow.Status.ToString(),
                Violations = validation.Violations.Select(v => v.Code).ToList(),
                QuotationId = quotationId,
                proposal.Replans
            });
        try
        {
            await unitOfWork.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            // A proposal the database rejects must still end the workflow; otherwise it would stay Planning forever.
            return await FailUnsavedProposalAsync(workflowId, ex, ct);
        }

        return new ProposalOutcomeResponse(workflow.Id, workflow.Status.ToString(), quotationId, validation);
    }

    /// <summary>Discards the rejected changes and records a safe failure instead (no quotation, no holds).</summary>
    private async Task<ProposalOutcomeResponse> FailUnsavedProposalAsync(Guid workflowId, DbUpdateException ex,
        CancellationToken ct)
    {
        unitOfWork.DiscardChanges();
        var workflow = (await workflows.GetByIdAsync(workflowId, ct))!;
        var trip = (await trips.GetByIdAsync(workflow.TripRequestId, ct))!;
        var reason = $"The proposal could not be saved ({ex.InnerException?.GetType().Name ?? ex.GetType().Name}).";
        var validation = new ProposalValidationResult(false, [new("PROPOSAL_NOT_SAVED", reason, ViolationSeverity.Hard)]);
        FailSafely(workflow, trip, reason);
        workflow.ValidationResult = WorkflowJson.Serialize(validation);
        audit.Record(null, "AgentWorkflowFailedSafely", nameof(AgentWorkflow), workflow.Id, null,
            new { Status = workflow.Status.ToString(), workflow.ErrorSummary });
        await unitOfWork.SaveChangesAsync(ct);
        return new ProposalOutcomeResponse(workflow.Id, workflow.Status.ToString(), null, validation);
    }

    /// <summary>Workflow and trip become FailedSafely; the tourist or manager can start planning again.</summary>
    private void FailSafely(AgentWorkflow workflow, TripRequest trip, string reason)
    {
        workflow.Status = AgentWorkflowStatus.FailedSafely;
        workflow.ErrorSummary = reason.Length > MaxErrorSummaryLength ? reason[..MaxErrorSummaryLength] : reason;
        workflow.FinishedAt = DateTime.UtcNow;
        workflow.CurrentStep = "failed";
        TripStatusMachine.Move(trip, TripRequestStatus.FailedSafely, null, workflow.ErrorSummary, audit);
    }

    private static string Shorten(string text) => text.Length <= 60 ? text : text[..57] + "...";

    public static QuotationDraft ToDraft(TripRequest trip, AgentWorkflow workflow, ProposalQuotation q, ProposalFacts facts,
        string snapshot) => new(
        trip.Id, workflow.Id, q.SubtotalLkr, q.MarginPct, q.TotalLkr, q.TotalUsd, q.FxRate, q.FxAsOf, q.FxStale,
        // A zero line (e.g. no transfer km on a one-city trip) prices nothing and is not stored, as in QuotationCalculator.
        (q.Lines ?? []).Where(l => l.Qty > 0).Select(l => new QuotationDraftLine(
                l.LineType, QuotationLineNames.Describe(l, facts), l.Qty, l.UnitLkr, l.AmountLkr))
            .ToList(),
        snapshot);
}
