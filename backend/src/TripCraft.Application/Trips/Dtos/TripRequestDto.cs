using System.Text.Json;

namespace TripCraft.Application.Trips.Dtos;

public record TripRequestDto(
    Guid Id,
    Guid TouristId,
    string Objective,
    DateOnly StartDate,
    DateOnly EndDate,
    int Pax,
    decimal BudgetUsd,
    JsonElement Preferences,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<string> Cities)
{
    public static TripRequestDto FromEntity(TripRequest t) => new(
        t.Id, t.TouristId, t.Objective, t.StartDate, t.EndDate, t.Pax, t.BudgetUsd,
        JsonDocument.Parse(t.Preferences).RootElement.Clone(),
        t.Status.ToString(), t.CreatedAt, t.UpdatedAt, t.CityList);
}
