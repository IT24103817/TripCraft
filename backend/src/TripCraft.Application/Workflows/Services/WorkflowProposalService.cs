using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Common;
using TripCraft.Application.Common.Auditing;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Common.Notifications;
using TripCraft.Application.Identity;
using TripCraft.Application.Trips;
using TripCraft.Application.Workflows.Dtos;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Application.Workflows.Services;

/// <summary>
/// Receives the agents' final proposal (PLAN.md section 6, step 7–8) and, in v1.1, sends it straight to the client.
/// 1. load the workflow and trip; only a Planning workflow accepts a proposal (also after Replan with note);
/// 2. load the database facts and run the deterministic ProposalValidator;
/// 3. valid, or only over budget after the agents' lowest-cost re-plans: stage a quotation version marked sent
///    (with the best-price flag when over budget), workflow Approved, trip Planning → QuotationSent, the tourist is
///    notified and emailed. A quote that failed a Hard rule is never sent;
/// 4. any Hard rule or an agent failure: workflow FailedSafely, trip → NeedsOperator, the managers are notified;
/// 5. store the validation result and proposal, audit, save in one transaction; send the email after the commit.
/// Nothing is held here — holds are created only by Confirm, the human approval gate.
/// </summary>
public class WorkflowProposalService(
    IAgentWorkflowRepository workflows,
    ITripRequestRepository trips,
    ProposalFactsLoader factsLoader,
    IQuotationStore quotations,
    ProposalValidator validator,
    INotifier notifier,
    IUserRepository users,
    IEmailDispatcher emails,
    IAuditLogger audit,
    IUnitOfWork unitOfWork) : IWorkflowProposalService
{
    private const int MaxErrorSummaryLength = 1000;

    public async Task<ProposalOutcomeResponse> ReceiveAsync(Guid workflowId, AgentProposalRequest proposal, CancellationToken ct)
    {
        // 1. Load.
        var workflow = await workflows.GetByIdAsync(workflowId, ct)
                       ?? throw new NotFoundException("Workflow not found.");
        if (workflow.Status != AgentWorkflowStatus.Planning)
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
            await FailSafelyAsync(workflow, trip, $"Agents failed safely: {reason}", ct);
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
                    await FailSafelyAsync(workflow, trip, $"Deterministic validation failed: {codes}", ct);
                }
                else
                {
                    quotationId = await SendToClientAsync(workflow, trip, proposal.Quotation!, facts, stored, validation, ct);
                }
            }
            catch (ComponentNotAvailableException ex)
            {
                validation = new ProposalValidationResult(false, [new("COMPONENT_UNAVAILABLE", ex.Message, ViolationSeverity.Hard)]);
                await FailSafelyAsync(workflow, trip, ex.Message, ct);
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

        if (quotationId is not null)
            await SendEmailsAsync(ct);
        return new ProposalOutcomeResponse(workflow.Id, workflow.Status.ToString(), quotationId, validation);
    }

    /// <summary>
    /// Auto-send (v1.1): the version is created already sent. Over budget (only possible after the agents' lowest-cost
    /// re-plans, as the sole Soft rule) it is sent anyway as the best price available, with how much it is over.
    /// </summary>
    private async Task<Guid> SendToClientAsync(AgentWorkflow workflow, TripRequest trip, ProposalQuotation quotation,
        ProposalFacts facts, StoredProposal stored, ProposalValidationResult validation, CancellationToken ct)
    {
        decimal? overBudgetUsd = validation.HasSoft ? Math.Max(0, Math.Round(quotation.TotalUsd - trip.BudgetUsd, 2)) : null;
        var quotationId = await quotations.AddVersionAsync(
            ToDraft(trip, workflow, quotation, facts, ProposalSnapshot.Serialize(stored)) with
            {
                OverBudgetUsd = overBudgetUsd,
                SendNow = true
            }, ct);
        workflow.Status = AgentWorkflowStatus.Approved;
        workflow.CurrentStep = "awaiting-client";
        var note = Quotations.Dtos.QuotationDto.BudgetNoteFor(overBudgetUsd);
        TripStatusMachine.Move(trip, TripRequestStatus.QuotationSent, null,
            note is null ? "Quotation sent to the client automatically." : $"Quotation sent to the client automatically. {note}.",
            audit);
        notifier.NotifyTourist(trip, "QuotationSent", "Your quotation is ready",
            $"USD {quotation.TotalUsd:N2}{(note is null ? "" : $" ({note})")}. Open the trip to accept or decline it.");
        if (trip.Tourist is not null && await users.GetByIdAsync(trip.Tourist.UserId, ct) is { } tourist)
            notifier.QueueEmail(tourist.Email, "Your TripCraft quotation is ready",
                $"Dear {tourist.FullName},\n\nYour quotation for {trip.StartDate:dd MMM yyyy} – {trip.EndDate:dd MMM yyyy} is ready: " +
                $"USD {quotation.TotalUsd:N2} (LKR {quotation.TotalLkr:N2}).{(note is null ? "" : $" {note}.")} " +
                "Open the TripCraft app to accept or decline it.\n\nTripCraft", trip.Id);
        return quotationId;
    }

    /// <summary>After the commit; a failed email stays in the outbox with its error and never undoes the quote.</summary>
    private async Task SendEmailsAsync(CancellationToken ct)
    {
        try
        {
            await emails.DispatchPendingAsync(ct);
        }
        catch (Exception)
        {
            // The quotation is sent in the app either way; the outbox row keeps the failure.
        }
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
        await FailSafelyAsync(workflow, trip, reason, ct);
        workflow.ValidationResult = WorkflowJson.Serialize(validation);
        audit.Record(null, "AgentWorkflowFailedSafely", nameof(AgentWorkflow), workflow.Id, null,
            new { Status = workflow.Status.ToString(), workflow.ErrorSummary });
        await unitOfWork.SaveChangesAsync(ct);
        return new ProposalOutcomeResponse(workflow.Id, workflow.Status.ToString(), null, validation);
    }

    /// <summary>
    /// Workflow FailedSafely, trip NeedsOperator: nothing is sent to the client. The managers are told; they retry
    /// planning, edit and send by hand, or cancel.
    /// </summary>
    private async Task FailSafelyAsync(AgentWorkflow workflow, TripRequest trip, string reason, CancellationToken ct)
    {
        workflow.Status = AgentWorkflowStatus.FailedSafely;
        workflow.ErrorSummary = reason.Length > MaxErrorSummaryLength ? reason[..MaxErrorSummaryLength] : reason;
        workflow.FinishedAt = DateTime.UtcNow;
        workflow.CurrentStep = "failed";
        TripStatusMachine.Move(trip, TripRequestStatus.NeedsOperator, null, workflow.ErrorSummary, audit);
        await notifier.NotifyManagersAsync("NeedsOperator", "A trip needs the operator",
            $"\"{Shorten(trip.Objective)}\": {workflow.ErrorSummary}", trip.Id, ct);
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
