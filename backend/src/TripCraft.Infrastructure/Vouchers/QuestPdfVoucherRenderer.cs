using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TripCraft.Application.Vouchers;

namespace TripCraft.Infrastructure.Vouchers;

/// <summary>
/// One A5 page per voucher: trip details, what the voucher is for, its QR code and the signed code in text.
/// QuestPDF Community licence (free for this project's size); QRCoder draws the QR as a PNG.
/// </summary>
public class QuestPdfVoucherRenderer : IVoucherPdfRenderer
{
    static QuestPdfVoucherRenderer() => QuestPDF.Settings.License = LicenseType.Community;

    public byte[] Render(TripVouchersDto trip) =>
        Document.Create(container =>
        {
            foreach (var voucher in trip.Vouchers)
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5);
                    page.Margin(30);
                    page.DefaultTextStyle(t => t.FontSize(11));
                    page.Header().Text("TripCraft voucher").FontSize(20).Bold().FontColor(Colors.Teal.Darken2);
                    page.Content().PaddingVertical(10).Column(column =>
                    {
                        column.Spacing(6);
                        column.Item().Text(Title(voucher)).FontSize(15).SemiBold();
                        column.Item().Text($"Guest: {trip.TouristName} · {trip.Pax} people");
                        column.Item().Text($"Trip: {trip.StartDate:dd MMM yyyy} – {trip.EndDate:dd MMM yyyy}" +
                                           (trip.Cities.Count > 0 ? $" · {string.Join(", ", trip.Cities)}" : ""));
                        if (voucher.Type == nameof(VoucherType.HotelNight))
                            column.Item().Text($"{voucher.HotelName ?? "Hotel"} · night of {voucher.Night:dd MMM yyyy} · {voucher.Rooms} room(s)");
                        column.Item().AlignCenter().Width(200).Image(QrPng(voucher.QrPayload));
                        column.Item().AlignCenter().Text(voucher.Code).FontSize(7).FontColor(Colors.Grey.Darken1);
                    });
                    page.Footer().AlignCenter().Text(voucher.Type == nameof(VoucherType.Trip)
                        ? "Show this code to your guide each day. The signature is checked when it is scanned."
                        : "Show this code at hotel check-in.").FontSize(9).FontColor(Colors.Grey.Darken1);
                });
            }
        }).GeneratePdf();

    private static string Title(VoucherDto voucher) =>
        voucher.Type == nameof(VoucherType.Trip) ? "Trip voucher" : "Hotel night voucher";

    private static byte[] QrPng(string payload)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.M);
        return new PngByteQRCode(data).GetGraphic(10);
    }
}
