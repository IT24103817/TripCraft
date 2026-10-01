using System.Text.Json;

namespace TripCraft.Application.Trips.Dtos;

/// <summary>
/// Submitted by a Tourist from Flutter. Nationality and passport number update the caller's
/// tourist profile; only the last 4 passport characters are stored.
/// </summary>
public record CreateTripRequestRequest(
    string Objective,
    DateOnly StartDate,
    DateOnly EndDate,
    int Pax,
    decimal BudgetUsd,
    JsonElement? Preferences,
    string Nationality,
    string PassportNumber,
    IReadOnlyList<string>? Cities) : ITripDetails;
