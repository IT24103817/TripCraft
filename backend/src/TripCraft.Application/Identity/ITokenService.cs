namespace TripCraft.Application.Identity;

public interface ITokenService
{
    /// <summary>Creates a signed JWT with sub, email and role claims.</summary>
    (string Token, DateTime ExpiresAt) CreateToken(User user);
}
