using FluentValidation;
using TripCraft.Application.Trips.Dtos;
using TripCraft.Application.Trips.Planning;

namespace TripCraft.Application.Trips.Validators;

/// <summary>Operator rule from PLAN.md section 5: a day has 1–3 stops, each visited once.</summary>
public class EditItineraryDayRequestValidator : AbstractValidator<EditItineraryDayRequest>
{
    public const int MaxNotesLength = 500;

    public EditItineraryDayRequestValidator()
    {
        RuleFor(r => r.AttractionIds)
            .NotNull()
            .Must(ids => ids.Count is >= 1 and <= TripPlanningRules.MaxStopsPerDay)
            .WithMessage($"A day needs 1 to {TripPlanningRules.MaxStopsPerDay} stops.")
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("Each attraction can only be visited once a day.");
        RuleForEach(r => r.AttractionIds).NotEmpty();
        RuleFor(r => r.Notes).MaximumLength(MaxNotesLength);
    }
}
