using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TripCraft.Api.Authorization;
using TripCraft.Application.Quotations;

namespace TripCraft.Api.Controllers.Quotations;

/// <summary>
/// The manager's review gate at PendingReview (PLAN.md section 6, step 9–10; v1.1 lifecycle). Operations Manager only:
/// a Tourist or Admin calling these gets 403 (separation of duties, PLAN.md section 2).
/// </summary>
[ApiController]
[Route("api/quotations")]
[Authorize(Roles = Roles.OperationsManager)]
public class QuotationApprovalsController(IQuotationApprovalService approvals) : ControllerBase
{
    /// <summary>"Send to client": PendingReview → QuotationSent and the tourist is notified. Nothing is held yet.</summary>
    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<QuotationDecisionResponse>> Approve(Guid id,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] QuotationDecisionRequest? request, CancellationToken ct) =>
        Ok(await approvals.ApproveAsync(User.GetCurrentUser(), id, request?.Comment, ct));

    /// <summary>The operator turns the trip down: PendingReview → Cancelled, the tourist is notified.</summary>
    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<QuotationDecisionResponse>> Reject(Guid id,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] QuotationDecisionRequest? request, CancellationToken ct) =>
        Ok(await approvals.RejectAsync(User.GetCurrentUser(), id, request?.Comment, ct));

    /// <summary>The comment is required and sent to the Planner agent, which re-plans the trip (new version).</summary>
    [HttpPost("{id:guid}/request-revision")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<QuotationDecisionResponse>> RequestRevision(Guid id, RequestRevisionRequest request,
        CancellationToken ct) =>
        Ok(await approvals.RequestRevisionAsync(User.GetCurrentUser(), id, request.Comment, ct));
}
