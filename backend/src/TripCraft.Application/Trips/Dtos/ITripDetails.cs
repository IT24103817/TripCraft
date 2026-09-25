using System.Text.Json;

namespace TripCraft.Application.Trips.Dtos;

/// <summary>Fields shared by create and update, so both can use TripDetailsValidator.</summary>
public interface ITripDetails
{
    string Objective { get; }
    DateOnly StartDate { get; }
    DateOnly EndDate { get; }
    int Pax { get; }
    decimal BudgetUsd { get; }
    JsonElement? Preferences { get; }
}
