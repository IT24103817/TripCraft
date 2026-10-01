namespace TripCraft.Application.Vouchers;

/// <summary>Draws the printable voucher sheet (one page per voucher, each with its QR code) as PDF bytes.</summary>
public interface IVoucherPdfRenderer
{
    byte[] Render(TripVouchersDto trip);
}
