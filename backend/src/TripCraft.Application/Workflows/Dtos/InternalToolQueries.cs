namespace TripCraft.Application.Workflows.Dtos;

/// <summary>Query strings of the internal tool endpoints used by the agent service.</summary>
public class CityQuery
{
    public string City { get; set; } = string.Empty;
}

public class DistanceQuery
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
}

public class WeatherQuery
{
    public string City { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
}

public class GuideAvailabilityQuery
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public string Language { get; set; } = string.Empty;
    public int Pax { get; set; }
}

public class VehicleAvailabilityQuery
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public int Seats { get; set; }
}

public class RoomAvailabilityQuery
{
    public string City { get; set; } = string.Empty;
    public DateOnly Night { get; set; }
    public int Rooms { get; set; }
}
