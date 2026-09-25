using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TripCraft.Api.Authorization;
using TripCraft.Application.Quotations;

namespace TripCraft.Api.Controllers.Quotations;

/// <summary>
/// The human approval gate (PLAN.md section 6, step 9–10). Operations Manager only:
/// a Tourist or Admin calling these gets 403 (separation of duties, PLAN.md section 2).
/// </summary>
[ApiController]
[Route("api/quotations")]
[Authorize(Roles = Roles.OperationsManager)]
public class QuotationApprovalsController(IQuotationApprovalService approvals) : ControllerBase
{
    /// <summary>One transaction: holds, quotation, trip, workflow, decision, audit. 409 and nothing saved on any failure.</summary>
    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult<QuotationDecisionResponse>> Approve(Guid id,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] QuotationDecisionRequest? request, CancellationToken ct) =>
        Ok(await approvals.ApproveAsync(User.GetCurrentUser(), id, request?.Comment, ct));

    [HttpPost("{id:guid}/reject")]
    public async Task<ActionResult<QuotationDecisionResponse>> Reject(Guid id,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] QuotationDecisionRequest? request, CancellationToken ct) =>
        Ok(await approvals.RejectAsync(User.GetCurrentUser(), id, request?.Comment, ct));

    /// <summary>The comment is sent to the Planner agent, which re-plans the trip.</summary>
    [HttpPost("{id:guid}/request-revision")]
    public async Task<ActionResult<QuotationDecisionResponse>> RequestRevision(Guid id, RequestRevisionRequest request,
        CancellationToken ct) =>
        Ok(await approvals.RequestRevisionAsync(User.GetCurrentUser(), id, request.Comment, ct));
}
