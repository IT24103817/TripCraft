using TripCraft.Application.Trips;
using TripCraft.Application.Workflows;

namespace TripCraft.Application.Quotations;

/// <summary>POST /api/quotations/{id}/send (manager, Edit &amp; resend / send manually). The comment is optional.</summary>
public record QuotationDecisionRequest(string? Comment);

/// <summary>POST /api/quotations/{id}/decline (tourist). The reason is required and shown to the manager.</summary>
public record DeclineQuotationRequest(string Reason);

/// <summary>POST /api/trip-requests/{id}/replan (manager, after a decline). The note goes to the Planner agent.</summary>
public record ReplanRequest(string Note);

public record QuotationDecisionResponse(Guid QuotationId, Guid TripRequestId, Guid WorkflowId, string Decision,
    string TripStatus, string WorkflowStatus, int HoldsCreated)
{
    public static QuotationDecisionResponse From(Guid quotationId, TripRequest trip, AgentWorkflow workflow, string decision,
        int holds) =>
        new(quotationId, trip.Id, workflow.Id, decision, trip.Status.ToString(), workflow.Status.ToString(), holds);
}
