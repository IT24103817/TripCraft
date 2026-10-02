using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripCraft.Api.Authorization;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Quotations;
using TripCraft.Application.Quotations.Dtos;
using TripCraft.Application.Quotations.Services;
using TripCraft.Application.Workflows.Dtos;
using TripCraft.Application.Workflows.Services;

namespace TripCraft.Api.Controllers.Quotations;

/// <summary>Component C — quotation list and detail, re-pricing, and the tourist accepting or declining a sent quotation.</summary>
[ApiController]
[Route("api/quotations")]
[Authorize(Roles = Roles.TouristOrOperationsManager)]
public class QuotationsController(IQuotationService quotations) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = Roles.OperationsManager)]
    public async Task<ActionResult<PagedResult<QuotationDto>>> List([FromQuery] QuotationListQuery query, CancellationToken ct) =>
        Ok(await quotations.ListAsync(query, ct));

    /// <summary>Lines, totals in LKR and USD, FX and the decisions. A tourist only sees their own trip's quotations.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<QuotationDto>> GetById(Guid id, CancellationToken ct) =>
        Ok(await quotations.GetAsync(User.GetCurrentUser(), id, ct));

    /// <summary>
    /// Business operation "Re-price": a new quotation version from the (edited) proposal with today's rate card
    /// and exchange rate, checked by ProposalValidator. The old version becomes Superseded. 409 on a Hard rule.
    /// </summary>
    [HttpPost("{id:guid}/calculate")]
    [Authorize(Roles = Roles.OperationsManager)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RepriceResponse>> Calculate(Guid id, [FromServices] IProposalEditService editor,
        CancellationToken ct) =>
        Ok(await editor.RepriceAsync(User.GetCurrentUser(), id, ct));

    /// <summary>Deposit paid / unpaid (manager), on the newest version after the client accepted it. 409 otherwise.</summary>
    [HttpPost("{id:guid}/payment")]
    [Authorize(Roles = Roles.OperationsManager)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<QuotationDto>> SetPayment(Guid id, SetPaymentRequest request, CancellationToken ct) =>
        Ok(await quotations.SetPaymentAsync(User.GetCurrentUser(), id, request.Paid, ct));

    /// <summary>The tourist accepts the quotation that was sent: QuotationSent → ClientAccepted, managers notified.</summary>
    [HttpPost("{id:guid}/accept")]
    [Authorize(Roles = Roles.Tourist)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<QuotationDecisionResponse>> Accept(Guid id, [FromServices] IQuotationClientService client,
        CancellationToken ct) =>
        Ok(await client.AcceptAsync(User.GetCurrentUser(), id, ct));

    /// <summary>The tourist declines with a reason: QuotationSent → ClientDeclined, the reason is shown to the manager.</summary>
    [HttpPost("{id:guid}/decline")]
    [Authorize(Roles = Roles.Tourist)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<QuotationDecisionResponse>> Decline(Guid id, DeclineQuotationRequest request,
        [FromServices] IQuotationClientService client, CancellationToken ct) =>
        Ok(await client.DeclineAsync(User.GetCurrentUser(), id, request.Reason, ct));
}
