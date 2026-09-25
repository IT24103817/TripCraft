using FluentValidation;
using TripCraft.Application.Trips.Dtos;

namespace TripCraft.Application.Trips.Validators;

public class SaveAttractionRequestValidator : AbstractValidator<SaveAttractionRequest>
{
    public SaveAttractionRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(50);
        RuleFor(x => x.DurationMinutes).InclusiveBetween(15, 600);
        RuleFor(x => x.EntryFeeLkr).GreaterThanOrEqualTo(0).LessThan(10_000_000_000m);
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180);
    }
}
