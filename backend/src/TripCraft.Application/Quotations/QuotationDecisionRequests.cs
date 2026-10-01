namespace TripCraft.Application.Quotations;

/// <summary>POST /api/quotations/{id}/approve and /reject. The comment is optional.</summary>
public record QuotationDecisionRequest(string? Comment);

/// <summary>POST /api/quotations/{id}/request-revision. The comment is required: it goes to the Planner agent.</summary>
public record RequestRevisionRequest(string Comment);

/// <summary>POST /api/quotations/{id}/decline (tourist). The reason is required and shown to the manager.</summary>
public record DeclineQuotationRequest(string Reason);

public record QuotationDecisionResponse(Guid QuotationId, Guid TripRequestId, Guid WorkflowId, string Decision,
    string TripStatus, string WorkflowStatus, int HoldsCreated);
