using TripCraft.Application.Workflows.Dtos;

namespace TripCraft.Application.Workflows;

/// <summary>
/// The agent's calculate_quotation tool only knows ids, so its guide, vehicle and room lines read like
/// "Guide 0000…a003". Before the quotation is stored, those descriptions are replaced with the names the
/// validator already loaded (the tourist sees these lines). Amounts are never changed. Pure.
/// </summary>
public static class QuotationLineNames
{
    public static string Describe(ProposalQuotationLine line, ProposalFacts facts) => line.LineType switch
    {
        "guide" when facts.Guide is { } g => $"Guide {g.Name}",
        "vehicle" when facts.Vehicle is { } v => $"{v.Type} {v.RegistrationNo}",
        "room" => facts.RoomTypes.Values
                      .Where(r => line.Description.Contains(r.RoomTypeId.ToString(), StringComparison.OrdinalIgnoreCase))
                      .Select(r => $"{r.HotelName} — {r.RoomTypeName}")
                      .FirstOrDefault()
                  ?? line.Description,
        _ => line.Description
    };
}
