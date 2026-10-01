using FluentValidation;

namespace TripCraft.Application.Vouchers;

/// <summary>One voucher; QrPayload is what the QR code shows (and what the guide's scanner sends back).</summary>
public record VoucherDto(Guid Id, string Type, Guid? HotelId, string? HotelName, DateOnly? Night, int Rooms, string Code,
    string QrPayload);

/// <summary>Everything the printable voucher sheet needs.</summary>
public record TripVouchersDto(Guid TripRequestId, string Objective, DateOnly StartDate, DateOnly EndDate, int Pax,
    string TouristName, IReadOnlyList<string> Cities, IReadOnlyList<VoucherDto> Vouchers);

/// <summary>POST /api/vouchers/verify.</summary>
public record VerifyVoucherRequest(string Code);

public record VoucherVerificationDto(bool Valid, string Message, Guid? TripRequestId, string? Type, DateOnly? Night);

public class VerifyVoucherRequestValidator : AbstractValidator<VerifyVoucherRequest>
{
    public VerifyVoucherRequestValidator() => RuleFor(r => r.Code).NotEmpty().MaximumLength(300);
}
