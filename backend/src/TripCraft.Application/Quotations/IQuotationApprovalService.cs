using TripCraft.Application.Common.Security;

namespace TripCraft.Application.Quotations;

public interface IQuotationApprovalService
{
    Task<QuotationDecisionResponse> ApproveAsync(CurrentUser user, Guid quotationId, string? comment, CancellationToken ct);
    Task<QuotationDecisionResponse> RejectAsync(CurrentUser user, Guid quotationId, string? comment, CancellationToken ct);
    Task<QuotationDecisionResponse> RequestRevisionAsync(CurrentUser user, Guid quotationId, string comment, CancellationToken ct);
}
