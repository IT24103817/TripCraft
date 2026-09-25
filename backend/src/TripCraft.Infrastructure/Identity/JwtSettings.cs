namespace TripCraft.Infrastructure.Identity;

/// <summary>Filled from the JWT_SECRET and JWT_ISSUER environment variables.</summary>
public class JwtSettings
{
    public const int ExpiryMinutes = 60;

    public required string Secret { get; init; }
    public required string Issuer { get; init; }
}
