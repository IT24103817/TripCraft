using System.Text.Json;
using FluentValidation;
using TripCraft.Application.Trips.Dtos;
using TripCraft.Application.Trips.Planning;

namespace TripCraft.Application.Trips.Validators;

/// <summary>Rules for the trip fields shared by create and update (PLAN.md section 6, step 2).</summary>
public class TripDetailsValidator : AbstractValidator<ITripDetails>
{
    public TripDetailsValidator()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        RuleFor(x => x.Objective).NotEmpty().Length(10, 2000);
        RuleFor(x => x.StartDate).GreaterThanOrEqualTo(today)
            .WithMessage("Start date cannot be in the past.");
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("End date must be on or after the start date.");
        RuleFor(x => x.EndDate)
            .Must((x, end) => TripPlanningRules.TripDays(x.StartDate, end) <= TripPlanningRules.MaxTripDays)
            .When(x => x.EndDate >= x.StartDate)
            .WithMessage($"Trips can be at most {TripPlanningRules.MaxTripDays} days.");
        RuleFor(x => x.Pax).InclusiveBetween(1, 50);
        RuleFor(x => x.BudgetUsd).GreaterThan(0).LessThanOrEqualTo(1_000_000);
        RuleFor(x => x.Preferences)
            .Must(p => p is null || p.Value.ValueKind == JsonValueKind.Object)
            .WithMessage("Preferences must be a JSON object.");
    }
}
