using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TripCraft.Api.Authorization;
using TripCraft.Application.Quotations.Documents;
using TripCraft.Application.Vouchers;

namespace TripCraft.Api.Controllers.Vouchers;

/// <summary>Signed vouchers issued when a trip is confirmed (v1.1).</summary>
[ApiController]
public class VouchersController(IVoucherService vouchers) : ControllerBase
{
    /// <summary>The trip voucher and one voucher per hotel night, with QR payloads. Owner tourist or a manager.</summary>
    [HttpGet("api/trips/{id:guid}/vouchers")]
    [Authorize(Roles = Roles.TouristOrOperationsManager)]
    public async Task<ActionResult<IReadOnlyList<VoucherDto>>> List(Guid id, CancellationToken ct) =>
        Ok(await vouchers.ListAsync(User.GetCurrentUser(), id, ct));

    /// <summary>Printable PDF, one page per voucher with its QR code. 409 before the trip is confirmed.</summary>
    [HttpGet("api/trips/{id:guid}/vouchers.pdf")]
    [Authorize(Roles = Roles.TouristOrOperationsManager)]
    [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK, "application/pdf")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Pdf(Guid id, CancellationToken ct) =>
        File(await vouchers.RenderPdfAsync(User.GetCurrentUser(), id, ct), "application/pdf", $"tripcraft-vouchers-{id:N}.pdf");

    /// <summary>
    /// Printable itinerary + quotation (with deposit) for the owner tourist or a manager. 409 until a quotation was sent.
    /// </summary>
    [HttpGet("api/trips/{id:guid}/itinerary.pdf")]
    [Authorize(Roles = Roles.TouristOrOperationsManager)]
    [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK, "application/pdf")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ItineraryPdf(Guid id, [FromServices] ITripDocumentService documents, CancellationToken ct) =>
        File(await documents.RenderItineraryAsync(User.GetCurrentUser(), id, ct), "application/pdf", $"tripcraft-itinerary-{id:N}.pdf");

    /// <summary>Checks a scanned code's signature and that it was issued (guide's scanner, hotel desk).</summary>
    [HttpPost("api/vouchers/verify")]
    [Authorize(Roles = Roles.GuideOrOperationsManager)]
    public async Task<ActionResult<VoucherVerificationDto>> Verify(VerifyVoucherRequest request, CancellationToken ct) =>
        Ok(await vouchers.VerifyAsync(request.Code, ct));
}
