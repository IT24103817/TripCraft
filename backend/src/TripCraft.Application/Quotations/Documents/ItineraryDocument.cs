namespace TripCraft.Application.Quotations.Documents;

public record ItineraryDocumentDay(int DayNumber, DateOnly Date, string City, IReadOnlyList<string> Stops, string? Notes);

public record ItineraryDocumentLine(string Description, decimal Qty, decimal UnitLkr, decimal AmountLkr);

public record ItineraryDocumentQuotation(int Version, IReadOnlyList<ItineraryDocumentLine> Lines, decimal SubtotalLkr,
    decimal MarginLkr, decimal TotalLkr, decimal TotalUsd, decimal FxRate, decimal DepositPct, decimal DepositLkr,
    decimal DepositUsd, bool DepositPaid, bool Accepted);

/// <summary>Everything the itinerary + quotation PDF shows (v1.1).</summary>
public record ItineraryDocument(string Objective, string TouristName, string TripStatus, DateOnly StartDate,
    DateOnly EndDate, int Pax, IReadOnlyList<string> Cities, bool ItineraryConfirmed, IReadOnlyList<ItineraryDocumentDay> Days,
    ItineraryDocumentQuotation Quotation);

/// <summary>Draws the document as PDF bytes (QuestPDF in Infrastructure).</summary>
public interface IItineraryPdfRenderer
{
    byte[] Render(ItineraryDocument document);
}
