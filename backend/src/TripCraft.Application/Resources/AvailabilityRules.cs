namespace TripCraft.Application.Resources;

/// <summary>Pure Resource Management rules (PLAN.md section 3 Component B), unit tested without a database.</summary>
public static class AvailabilityRules
{
    /// <summary>A guide may check in only within this distance of the stop (PLAN.md section 8).</summary>
    public const int MaxCheckInDistanceMeters = 500;

    /// <summary>Inclusive date ranges overlap when each starts on or before the other ends.</summary>
    public static bool Overlaps(DateOnly aFrom, DateOnly aTo, DateOnly bFrom, DateOnly bTo) =>
        aFrom <= bTo && bFrom <= aTo;

    /// <summary>Rooms of a type still free on a night: never negative.</summary>
    public static int FreeRooms(int totalRooms, IEnumerable<int> heldQuantities) =>
        Math.Max(0, totalRooms - heldQuantities.Sum());

    /// <summary>Rooms of `capacity` people needed to sleep `pax` people.</summary>
    public static int RoomsNeeded(int pax, int capacity) =>
        capacity <= 0 ? pax : (pax + capacity - 1) / capacity;

    /// <summary>Great-circle distance in metres (haversine), accurate enough for a 500 m check-in rule.</summary>
    public static int DistanceMeters(double lat1, double lon1, double lat2, double lon2)
    {
        const double earthRadius = 6_371_000;
        double Rad(double degrees) => degrees * Math.PI / 180;
        var dLat = Rad(lat2 - lat1);
        var dLon = Rad(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                + Math.Cos(Rad(lat1)) * Math.Cos(Rad(lat2)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return (int)Math.Round(earthRadius * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a)));
    }
}
