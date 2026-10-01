using System.Text.Json;
using FluentValidation;

namespace TripCraft.Application.Trips.Templates;

public record TemplateStopDto(Guid? AttractionId, string Name, decimal EntryFeeLkr, double? Latitude, double? Longitude);

public record TemplateDayDto(int Day, string City, IReadOnlyList<TemplateStopDto> Stops);

/// <summary>A package card (list) or the full package (detail, with Days). FromPrice is for PricedForPax people.</summary>
public record TripTemplateDto(Guid Id, string Slug, string Name, string MoodTag, string Summary, string Objective,
    int Days, IReadOnlyList<string> Cities, JsonElement Preferences, int PricedForPax, decimal FromPriceLkr,
    decimal FromPriceUsd, IReadOnlyList<TemplateDayDto>? Itinerary);

/// <summary>POST /api/trip-templates/{id}/book: "Book as is". The trip runs the normal workflow from here.</summary>
public record BookTemplateRequest(DateOnly StartDate, int Pax, decimal BudgetUsd, string Nationality, string PassportNumber);

public class BookTemplateRequestValidator : AbstractValidator<BookTemplateRequest>
{
    public BookTemplateRequestValidator()
    {
        RuleFor(x => x.StartDate).GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Start date cannot be in the past.");
        RuleFor(x => x.Pax).InclusiveBetween(1, 50);
        RuleFor(x => x.BudgetUsd).GreaterThan(0).LessThanOrEqualTo(1_000_000);
        RuleFor(x => x.Nationality).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PassportNumber).NotEmpty()
            .Matches("^[A-Za-z0-9 ]{6,20}$").WithMessage("Passport number must be 6–20 letters or digits.");
    }
}
