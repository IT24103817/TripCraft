using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TripCraft.Application.Identity;
using TripCraft.Application.Quotations;
using TripCraft.Application.Trips;
using TripCraft.Application.Workflows.Ports;
using TripCraft.Infrastructure.Persistence;

namespace TripCraft.Infrastructure.Quotations;

/// <summary>The approved, accepted quotation of the completed sample trip, so the reports have data. Runs once.</summary>
public static class QuotationsSeeder
{
    public static async Task SeedAsync(AppDbContext db, ILogger logger, CancellationToken ct)
    {
        if (await db.Quotations.AnyAsync(ct))
            return;
        var trip = await db.TripRequests.FirstOrDefaultAsync(t => t.Status == TripRequestStatus.Completed, ct);
        var manager = await db.Users.FirstOrDefaultAsync(u => u.Role == UserRole.OperationsManager, ct);
        if (trip is null || manager is null)
            return;

        // 3 days, guide 6000/day, van 140 km x 120, 2 room-nights x 12000; 15 % margin; 300 LKR per USD.
        var items = new[]
        {
            new PriceItem("guide", "Guide Nimal Perera, 3 days", 3, 6000),
            new PriceItem("vehicle", "Van CAB-1234, 140 km", 140, 120),
            new PriceItem("room", "Kandy Hills — Standard Double, 1 room-night", 1, 12000),
            new PriceItem("room", "Ella Gap — Standard Double, 1 room-night", 1, 12000),
            new PriceItem("entry", "Royal Botanical Gardens entry, 2 people", 2, 3000),
            new PriceItem("entry", "Temple of the Sacred Tooth Relic entry, 2 people", 2, 2000)
        };
        var price = QuotationCalculator.Calculate(items, 15m, 300m);
        var quotation = new Quotation
        {
            TripRequestId = trip.Id, Version = 1, SubtotalLkr = price.SubtotalLkr, MarginPct = price.MarginPct,
            TotalLkr = price.TotalLkr, TotalUsd = price.TotalUsd, FxRate = price.FxRate,
            FxAsOf = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc), Status = QuotationStatus.Approved,
            AcceptedAt = new DateTime(2026, 8, 2, 9, 0, 0, DateTimeKind.Utc),
            Lines = price.Lines.Select(l => new QuotationLine
            {
                LineType = l.LineType, Description = l.Description, Qty = l.Qty, UnitLkr = l.UnitLkr, AmountLkr = l.AmountLkr
            }).ToList()
        };
        db.Quotations.Add(quotation);
        // v1.1: the booking is the Confirm decision (the approval gate); revenue reports count it.
        db.ApprovalDecisions.Add(new ApprovalDecision
        {
            QuotationId = quotation.Id, DecidedBy = manager.Id, Decision = QuotationDecision.Confirmed,
            Comment = "Sample confirmed trip", DecidedAt = new DateTime(2026, 8, 1, 10, 0, 0, DateTimeKind.Utc)
        });
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seeded the sample trip's approved quotation ({TotalUsd} USD)", price.TotalUsd);
    }
}
