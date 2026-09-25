using Microsoft.Extensions.Configuration;
using TripCraft.Application.Trips;

namespace TripCraft.Infrastructure.Persistence;

/// <summary>
/// Saves passport photos to a private folder: UPLOADS_DIR, default "uploads" next to the app.
/// The folder is never exposed by the web server. Swap for blob storage in production.
/// </summary>
public class LocalPassportPhotoStore(IConfiguration configuration) : IPassportPhotoStore
{
    public const string Folder = "passport-photos";

    public async Task<string> SaveAsync(Stream content, string extension, CancellationToken ct)
    {
        var root = configuration["UPLOADS_DIR"] ?? Path.Combine(AppContext.BaseDirectory, "uploads");
        var directory = Path.Combine(root, Folder);
        Directory.CreateDirectory(directory);

        var fileName = $"{Guid.NewGuid():N}{extension}"; // random name: nothing about the tourist leaks
        await using var file = File.Create(Path.Combine(directory, fileName));
        await content.CopyToAsync(file, ct);
        return $"{Folder}/{fileName}";
    }
}
