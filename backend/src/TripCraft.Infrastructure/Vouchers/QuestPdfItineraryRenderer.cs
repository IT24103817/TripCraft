using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TripCraft.Application.Quotations.Documents;

namespace TripCraft.Infrastructure.Vouchers;

/// <summary>A4 itinerary + quotation: trip header, one block per day, then the priced lines, totals and deposit.</summary>
public class QuestPdfItineraryRenderer : IItineraryPdfRenderer
{
    static QuestPdfItineraryRenderer() => QuestPDF.Settings.License = LicenseType.Community;

    public byte[] Render(ItineraryDocument d) =>
        Document.Create(container => container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(36);
            page.DefaultTextStyle(t => t.FontSize(10));
            page.Header().Column(header =>
            {
                header.Item().Text("TripCraft — itinerary and quotation").FontSize(18).Bold().FontColor(Colors.Teal.Darken2);
                header.Item().Text($"{d.TouristName} · {d.Pax} people · {d.StartDate:dd MMM yyyy} – {d.EndDate:dd MMM yyyy}" +
                                   (d.Cities.Count > 0 ? $" · {string.Join(", ", d.Cities)}" : ""));
                header.Item().Text($"Status: {d.TripStatus}" + (d.ItineraryConfirmed ? " · confirmed itinerary" : " · proposed itinerary"))
                    .FontColor(Colors.Grey.Darken1);
            });
            page.Content().PaddingVertical(12).Column(column =>
            {
                column.Spacing(8);
                column.Item().Text(d.Objective).Italic();
                foreach (var day in d.Days)
                {
                    column.Item().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingBottom(6).Column(block =>
                    {
                        block.Item().Text($"Day {day.DayNumber} · {day.Date:ddd dd MMM} · {day.City}").SemiBold();
                        foreach (var stop in day.Stops)
                            block.Item().Text($"• {stop}");
                        if (!string.IsNullOrWhiteSpace(day.Notes))
                            block.Item().Text(day.Notes).FontColor(Colors.Grey.Darken1);
                    });
                }

                var q = d.Quotation;
                column.Item().PaddingTop(10).Text($"Quotation (version {q.Version})").FontSize(13).SemiBold();
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(c => { c.RelativeColumn(5); c.RelativeColumn(1); c.RelativeColumn(2); c.RelativeColumn(2); });
                    table.Header(h =>
                    {
                        h.Cell().Text("Item").SemiBold();
                        h.Cell().AlignRight().Text("Qty").SemiBold();
                        h.Cell().AlignRight().Text("Unit LKR").SemiBold();
                        h.Cell().AlignRight().Text("Amount LKR").SemiBold();
                    });
                    foreach (var line in q.Lines)
                    {
                        table.Cell().Text(line.Description);
                        table.Cell().AlignRight().Text($"{line.Qty:0.##}");
                        table.Cell().AlignRight().Text($"{line.UnitLkr:N2}");
                        table.Cell().AlignRight().Text($"{line.AmountLkr:N2}");
                    }
                });
                column.Item().AlignRight().Text($"Subtotal LKR {q.SubtotalLkr:N2} · margin LKR {q.MarginLkr:N2}");
                column.Item().AlignRight().Text($"Total LKR {q.TotalLkr:N2} = USD {q.TotalUsd:N2} (1 USD = {q.FxRate:N2} LKR)").Bold();
                column.Item().AlignRight().Text($"Deposit {q.DepositPct:0.##}%: LKR {q.DepositLkr:N2} (USD {q.DepositUsd:N2}) — " +
                                                (q.DepositPaid ? "paid" : "not paid yet"));
                column.Item().AlignRight().Text(q.Accepted ? "Accepted by the client." : "Waiting for the client's acceptance.")
                    .FontColor(Colors.Grey.Darken1);
            });
            page.Footer().AlignCenter().Text(t =>
            {
                t.Span("TripCraft · page ");
                t.CurrentPageNumber();
            });
        })).GeneratePdf();
}
