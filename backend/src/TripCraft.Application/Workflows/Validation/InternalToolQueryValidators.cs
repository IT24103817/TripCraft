using FluentValidation;
using TripCraft.Application.Workflows.Dtos;

namespace TripCraft.Application.Workflows.Validation;

internal static class CityRules
{
    public const string Pattern = @"^[A-Za-z .'-]+$";

    public static IRuleBuilderOptions<T, string> ValidCity<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().MaximumLength(60).Matches(Pattern);
}

public class CityQueryValidator : AbstractValidator<CityQuery>
{
    public CityQueryValidator() => RuleFor(q => q.City).ValidCity();
}

public class DistanceQueryValidator : AbstractValidator<DistanceQuery>
{
    public DistanceQueryValidator()
    {
        RuleFor(q => q.From).ValidCity();
        RuleFor(q => q.To).ValidCity();
    }
}

public class WeatherQueryValidator : AbstractValidator<WeatherQuery>
{
    public WeatherQueryValidator()
    {
        RuleFor(q => q.City).ValidCity();
        RuleFor(q => q.Date).NotEmpty();
    }
}

public class GuideAvailabilityQueryValidator : AbstractValidator<GuideAvailabilityQuery>
{
    public GuideAvailabilityQueryValidator()
    {
        RuleFor(q => q.From).NotEmpty();
        RuleFor(q => q.To).NotEmpty().GreaterThanOrEqualTo(q => q.From);
        RuleFor(q => q.Language).NotEmpty().Matches("^[a-z]{2}$");
        RuleFor(q => q.Pax).InclusiveBetween(1, 50);
    }
}

public class VehicleAvailabilityQueryValidator : AbstractValidator<VehicleAvailabilityQuery>
{
    public VehicleAvailabilityQueryValidator()
    {
        RuleFor(q => q.From).NotEmpty();
        RuleFor(q => q.To).NotEmpty().GreaterThanOrEqualTo(q => q.From);
        RuleFor(q => q.Seats).InclusiveBetween(1, 60);
    }
}

public class RoomAvailabilityQueryValidator : AbstractValidator<RoomAvailabilityQuery>
{
    public RoomAvailabilityQueryValidator()
    {
        RuleFor(q => q.City).ValidCity();
        RuleFor(q => q.Night).NotEmpty();
        RuleFor(q => q.Rooms).InclusiveBetween(1, 30);
    }
}
