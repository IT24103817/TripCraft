using TripCraft.Application.Common.Paging;

namespace TripCraft.Application.Resources.Dtos;

public record VehicleDto(Guid Id, string RegistrationNo, string Type, int Seats, decimal RatePerKmLkr, bool IsActive,
    DateTime CreatedAt, DateTime UpdatedAt)
{
    public static VehicleDto FromEntity(Vehicle v) =>
        new(v.Id, v.RegistrationNo, v.Type, v.Seats, v.RatePerKmLkr, v.IsActive, v.CreatedAt, v.UpdatedAt);
}

public record SaveVehicleRequest(string RegistrationNo, string Type, int Seats, decimal RatePerKmLkr, bool IsActive);

/// <summary>GET /api/vehicles?search=&amp;type=&amp;minSeats=&amp;sort=&amp;page=&amp;pageSize=</summary>
public class VehicleListQuery : PagedQuery
{
    public string? Type { get; set; }
    public int? MinSeats { get; set; }
}
