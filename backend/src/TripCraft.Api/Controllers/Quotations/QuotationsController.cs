using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripCraft.Api.Authorization;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Quotations.Dtos;
using TripCraft.Application.Quotations.Services;

namespace TripCraft.Api.Controllers.Quotations;

/// <summary>Component C — quotation list and detail, re-pricing, and the tourist accepting an approved price.</summary>
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

    /// <summary>Business operation: re-price a Pending quotation with today's rate card and exchange rate.</summary>
    [HttpPost("{id:guid}/calculate")]
    [Authorize(Roles = Roles.OperationsManager)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RecalculationDto>> Calculate(Guid id, CancellationToken ct) =>
        Ok(await quotations.RecalculateAsync(User.GetCurrentUser(), id, ct));

    /// <summary>The tourist accepts the approved price. 409 unless Approved, or when already accepted.</summary>
    [HttpPost("{id:guid}/accept")]
    [Authorize(Roles = Roles.Tourist)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<QuotationDto>> Accept(Guid id, CancellationToken ct) =>
        Ok(await quotations.AcceptAsync(User.GetCurrentUser(), id, ct));
}
