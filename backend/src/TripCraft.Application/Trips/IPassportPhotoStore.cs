namespace TripCraft.Application.Trips;

/// <summary>
/// Private storage for passport photos (PLAN.md section 10). Files get a random name and are never
/// served as static files; only the storage key is kept on the tourist profile.
/// </summary>
public interface IPassportPhotoStore
{
    /// <summary>Saves the photo and returns its storage key, e.g. "passport-photos/3f2c….jpg".</summary>
    Task<string> SaveAsync(Stream content, string extension, CancellationToken ct);
}
