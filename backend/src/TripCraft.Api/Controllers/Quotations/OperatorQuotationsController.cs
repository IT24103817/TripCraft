using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TripCraft.Api.Authorization;
using TripCraft.Application.Quotations;
using TripCraft.Application.Workflows.Dtos;
using TripCraft.Application.Workflows.Services;

namespace TripCraft.Api.Controllers.Quotations;

/// <summary>
/// The Operations Manager's exceptions to the automatic flow (v1.1): quotations reach the client without a manager,
/// and the human approval gate is Confirm. Operations Manager only: a Tourist or Admin gets 403.
/// </summary>
[ApiController]
[Authorize(Roles = Roles.OperationsManager)]
public class OperatorQuotationsController(IOperatorQuotationService operatorQuotations) : ControllerBase
{
    /// <summary>
    /// Sends a re-priced version (Edit &amp; resend at ClientAccepted, Edit &amp; send manually at NeedsOperator) →
    /// QuotationSent; the tourist must accept again. 409 if edited since priced, not the newest, or a Hard rule failed.
    /// </summary>
    [HttpPost("api/quotations/{id:guid}/send")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<QuotationDecisionResponse>> Send(Guid id,
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] QuotationDecisionRequest? request, CancellationToken ct) =>
        Ok(await operatorQuotations.SendAsync(User.GetCurrentUser(), id, request?.Comment, ct));

    /// <summary>After the client declined: the Planner re-plans with the note and the client's reason (→ Planning).</summary>
    [HttpPost("api/trip-requests/{id:guid}/replan")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<QuotationDecisionResponse>> Replan(Guid id, ReplanRequest request, CancellationToken ct) =>
        Ok(await operatorQuotations.ReplanAsync(User.GetCurrentUser(), id, request.Note, ct));

    /// <summary>Re-price by trip (also when no quotation exists yet, e.g. NeedsOperator after a Hard rule).</summary>
    [HttpPost("api/trip-requests/{id:guid}/proposal/reprice")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RepriceResponse>> Reprice(Guid id, [FromServices] IProposalEditService editor,
        CancellationToken ct) =>
        Ok(await editor.RepriceTripAsync(User.GetCurrentUser(), id, ct));
}
