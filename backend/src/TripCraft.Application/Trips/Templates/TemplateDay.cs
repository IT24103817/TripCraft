namespace TripCraft.Application.Trips.Templates;

/// <summary>One day of a package: the city it is spent in and the attractions (by name) visited.</summary>
public record TemplateDay(int Day, string City, IReadOnlyList<string> Stops);
