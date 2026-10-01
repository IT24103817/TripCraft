using TripCraft.Application.Common.Entities;

namespace TripCraft.Application.Vouchers;

/// <summary>
/// A signed voucher created when the manager confirms a trip (v1.1): one Trip voucher (the guide scans it to
/// check the tourists in) and one HotelNight voucher per hotel and night (shown at the hotel). Code carries an
/// HMAC signature (<see cref="VoucherSigner"/>), so a forged or edited code is rejected.
/// </summary>
public class Voucher : BaseEntity
{
    public Guid TripRequestId { get; set; }
    public VoucherType Type { get; set; }
    public Guid? HotelId { get; set; }
    public DateOnly? Night { get; set; }
    public int Rooms { get; set; }
    public string Code { get; set; } = string.Empty;

    /// <summary>What the QR code contains.</summary>
    public string QrPayload => VoucherSigner.QrPrefix + Code;
}

public enum VoucherType
{
    Trip,
    HotelNight
}
