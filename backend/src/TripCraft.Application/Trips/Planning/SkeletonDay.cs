namespace TripCraft.Application.Trips.Planning;

/// <summary>One day of the itinerary skeleton: which city, and how many stops the pace allows.</summary>
public record SkeletonDay(int DayNumber, DateOnly Date, string City, int MaxStops);
