using System.Text.Json;
using TripCraft.Application.Workflows.Dtos;
using TripCraft.Tests.Workflows.Fakes;
using S = TripCraft.Tests.Workflows.Fakes.FakeResourcesState;

namespace TripCraft.Tests.Workflows;

/// <summary>The PLAN.md section 6 demo as an agent proposal: 5 days, 4 pax, Kandy then Ella by train.</summary>
public record DemoAttractions(Guid Temple, Guid Peradeniya, Guid NineArches, Guid AdamsPeak)
{
    public static readonly DemoAttractions Random = new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

    /// <summary>Entry fees as seeded: Temple 2000, Peradeniya 3000, the Ella ones free.</summary>
    public IReadOnlyDictionary<Guid, decimal> Fees => new Dictionary<Guid, decimal>
    {
        [Temple] = 2000m, [Peradeniya] = 3000m, [NineArches] = 0m, [AdamsPeak] = 0m
    };
}

public static class TestProposals
{
    /// <summary>
    /// guide 5 x 6000 = 30000; van 140 km x 120 = 16800; rooms 8 x 12000 = 96000; entry (2000 + 3000) x 4 = 20000
    /// subtotal 162800; margin 15% = 24420; total 187220 LKR = 624.07 USD at 300.
    /// </summary>
    public const decimal GoldenTotalLkr = 187220m;

    public static AgentProposalRequest Golden(DateOnly start, DemoAttractions a, decimal totalLkr = GoldenTotalLkr) => new(
        Plan: JsonDocument.Parse("""{"plan":[{"step":1,"agent":"itinerary"}],"constraints":{"cities":["Kandy","Ella"]}}""").RootElement,
        Days:
        [
            Day(1, start, "Kandy", 0, Stop(a.Temple, 2000)),
            Day(2, start.AddDays(1), "Kandy", 0, Stop(a.Peradeniya, 3000)),
            Day(3, start.AddDays(2), "Ella", 140, Stop(a.NineArches, 0)),
            Day(4, start.AddDays(3), "Ella", 0, Stop(a.AdamsPeak, 0)),
            Day(5, start.AddDays(4), "Ella", 0, Stop(a.NineArches, 0))
        ],
        Resources: new ProposalResources(S.GuideEn.ToString(), S.VanSixSeats.ToString(),
        [
            Room(S.KandyHotel, S.KandyStandard, start), Room(S.KandyHotel, S.KandyStandard, start),
            Room(S.KandyHotel, S.KandyStandard, start.AddDays(1)), Room(S.KandyHotel, S.KandyStandard, start.AddDays(1)),
            Room(S.EllaHotel, S.EllaStandard, start.AddDays(2)), Room(S.EllaHotel, S.EllaStandard, start.AddDays(2)),
            Room(S.EllaHotel, S.EllaStandard, start.AddDays(3)), Room(S.EllaHotel, S.EllaStandard, start.AddDays(3))
        ], []),
        Quotation: new ProposalQuotation(
            [new ProposalQuotationLine("guide", "Guide", 5, 6000, 30000)],
            162800m, 15m, 24420m, totalLkr, 300m, new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc), false,
            Math.Round(totalLkr / 300m, 2)),
        Violations: [],
        Status: "PendingApproval",
        Replans: 0,
        ErrorSummary: null);

    public static ProposalDay Day(int n, DateOnly date, string city, decimal km, params ProposalStop[] stops) =>
        new(n, date, city, [.. stops], km > 0 ? "train" : "road", km, 0, null);

    public static ProposalStop Stop(Guid id, decimal fee) => new(id.ToString(), "Stop", fee);

    public static ProposalRoom Room(Guid hotel, Guid roomType, DateOnly night) => new(hotel.ToString(), roomType.ToString(), night);
}
