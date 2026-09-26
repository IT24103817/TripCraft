using TripCraft.Application.Common.Entities;

namespace TripCraft.Application.Resources;

public class Vehicle : BaseEntity
{
    public string RegistrationNo { get; set; } = string.Empty;

    /// <summary>Car, Van or Coach.</summary>
    public string Type { get; set; } = string.Empty;

    public int Seats { get; set; }
    public decimal RatePerKmLkr { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
}
