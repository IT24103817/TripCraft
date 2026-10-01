using System.Text.Json;

namespace TripCraft.Application.Trips.Dtos;

/// <summary>Edits the trip details. Allowed only while the request is Submitted or FailedSafely.</summary>
public record UpdateTripRequestRequest(
    string Objective,
    DateOnly StartDate,
    DateOnly EndDate,
    int Pax,
    decimal BudgetUsd,
    JsonElement? Preferences,
    IReadOnlyList<string>? Cities) : ITripDetails;
