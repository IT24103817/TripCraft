using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TripCraft.Application.Trips;
using TripCraft.Application.Trips.Templates;
using TripCraft.Application.Workflows;
using TripCraft.Infrastructure.Persistence;

namespace TripCraft.Infrastructure.Trips;

/// <summary>
/// The five mood packages on the tourist's home screen (v1.1). Seeded when the table is empty. Every stop is a seeded
/// attraction in that day's city, at most 3 a day, and each city change is within the 4 h driving rule.
/// </summary>
public static class TripTemplatesSeeder
{
    public static async Task SeedAsync(AppDbContext db, ILogger logger, CancellationToken ct)
    {
        if (await db.TripTemplates.AnyAsync(ct))
            return;

        db.TripTemplates.AddRange(
            Template(1, "hill-country-escape", "Hill-country escape", "Cool & green",
                "Tea hills, misty lakes and the famous Nine Arches Bridge, at an easy pace.",
                "4 relaxed days through the hill country: Kandy's temple, Nuwara Eliya's tea estates and Ella's viewpoints.",
                """{"language":"en","transport":"any","pace":"relaxed"}""",
                Day(1, "Kandy", "Temple of the Sacred Tooth Relic", "Kandy Lake"),
                Day(2, "Nuwara Eliya", "Pedro Tea Estate", "Gregory Lake"),
                Day(3, "Ella", "Nine Arches Bridge", "Little Adam's Peak"),
                Day(4, "Ella", "Ravana Falls")),
            Template(2, "beach-and-heritage", "Beach and heritage", "Sun & history",
                "Colombo's temples, the Dutch fort of Galle and a beach day in Unawatuna.",
                "4 days from Colombo to Galle: city temples, Galle Fort and lighthouse, Jungle Beach and the turtle hatchery.",
                """{"language":"en","transport":"road","pace":"relaxed"}""",
                Day(1, "Colombo", "Gangaramaya Temple", "Galle Face Green"),
                Day(2, "Galle", "Galle Fort", "Galle Lighthouse"),
                Day(3, "Galle", "Jungle Beach, Unawatuna", "Japanese Peace Pagoda, Rumassala"),
                Day(4, "Galle", "Koggala Sea Turtle Hatchery")),
            Template(3, "wildlife-weekend", "Wildlife weekend", "Wild & outdoors",
                "Horton Plains at dawn, World's End and waterfall hikes around Ella.",
                "A 3-day outdoor weekend: Horton Plains and World's End, Gregory Lake, then Ella Rock and Ravana Falls.",
                """{"language":"en","transport":"road","pace":"active"}""",
                Day(1, "Nuwara Eliya", "Horton Plains and World's End"),
                Day(2, "Nuwara Eliya", "Gregory Lake"),
                Day(3, "Ella", "Ella Rock", "Ravana Falls")),
            Template(4, "culture-triangle", "Culture triangle", "Ancient wonders",
                "Sigiriya's lion rock, the Dambulla cave temple and the royal city of Kandy.",
                "4 days of history: Sigiriya Rock Fortress, Dambulla Cave Temple and Pidurangala, then Kandy and Peradeniya.",
                """{"language":"en","transport":"road","pace":"normal"}""",
                Day(1, "Sigiriya", "Sigiriya Rock Fortress"),
                Day(2, "Sigiriya", "Dambulla Cave Temple", "Pidurangala Rock"),
                Day(3, "Kandy", "Temple of the Sacred Tooth Relic", "Bahirawakanda Vihara Buddha Statue"),
                Day(4, "Kandy", "Royal Botanical Gardens, Peradeniya")),
            Template(5, "slow-train-journey", "Slow train journey", "Slow & scenic",
                "The hill-country train from Kandy to Ella, with time to stop and look.",
                "5 slow days by the hill-country train: Kandy, the tea country of Nuwara Eliya and Ella.",
                """{"language":"en","transport":"train","pace":"relaxed"}""",
                Day(1, "Kandy", "Temple of the Sacred Tooth Relic", "Kandy Lake"),
                Day(2, "Kandy", "Royal Botanical Gardens, Peradeniya"),
                Day(3, "Nuwara Eliya", "Pedro Tea Estate", "Gregory Lake"),
                Day(4, "Ella", "Nine Arches Bridge"),
                Day(5, "Ella", "Little Adam's Peak")));
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seeded 5 trip templates");
    }

    private static TemplateDay Day(int day, string city, params string[] stops) => new(day, city, stops);

    private static TripTemplate Template(int order, string slug, string name, string mood, string summary,
        string objective, string preferences, params TemplateDay[] days) => new()
    {
        Slug = slug, Name = name, MoodTag = mood, Summary = summary, Objective = objective, Preferences = preferences,
        Days = days.Length, SortOrder = order, Plan = WorkflowJson.Serialize(days),
        Cities = TripRequest.JoinCities(days.Select(d => d.City).Distinct())
    };
}
