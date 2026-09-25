using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TripCraft.Application.Identity;

namespace TripCraft.Infrastructure.Persistence.Seeding;

/// <summary>
/// Inserts demo data on startup. Each part only runs when its own table is empty, so it is safe to run every time.
/// Users: 3 per role, tourist1@tripcraft.test ... admin3@tripcraft.test, password Passw0rd!
/// </summary>
public class DataSeeder(AppDbContext db, IPasswordHasher<User> passwordHasher, ILogger<DataSeeder> logger)
{
    public const string DemoPassword = "Passw0rd!";

    private static readonly (UserRole Role, string Prefix, string Label)[] Roles =
    [
        (UserRole.Tourist, "tourist", "Tourist"),
        (UserRole.Guide, "guide", "Guide"),
        (UserRole.OperationsManager, "manager", "Operations Manager"),
        (UserRole.Admin, "admin", "Admin")
    ];

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await SeedUsersAsync(ct);
        await TripsSeeder.SeedAsync(db, logger, ct);
    }

    private async Task SeedUsersAsync(CancellationToken ct)
    {
        if (await db.Users.AnyAsync(ct))
            return;

        foreach (var (role, prefix, label) in Roles)
        {
            for (var i = 1; i <= 3; i++)
            {
                var user = new User
                {
                    Email = $"{prefix}{i}@tripcraft.test",
                    FullName = $"Demo {label} {i}",
                    Role = role,
                    IsActive = true
                };
                user.PasswordHash = passwordHasher.HashPassword(user, DemoPassword);
                db.Users.Add(user);
            }
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seeded {Count} demo users", Roles.Length * 3);
    }
}
