using FluentValidation;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Resources.Dtos;
using TripCraft.Application.Resources.Services;

namespace TripCraft.Application.Resources.Validators;

public class SaveVehicleRequestValidator : AbstractValidator<SaveVehicleRequest>
{
    public SaveVehicleRequestValidator()
    {
        RuleFor(x => x.RegistrationNo).NotEmpty().Length(3, 20)
            .Matches("^[A-Za-z0-9-]+$").WithMessage("Registration may contain letters, digits and '-'.");
        RuleFor(x => x.Type).Must(t => VehicleService.Types.Contains(t))
            .WithMessage($"Type must be one of: {string.Join(", ", VehicleService.Types)}.");
        RuleFor(x => x.Seats).InclusiveBetween(1, 60);
        RuleFor(x => x.RatePerKmLkr).GreaterThan(0).LessThanOrEqualTo(10_000);
    }
}

public class VehicleListQueryValidator : AbstractValidator<VehicleListQuery>
{
    public VehicleListQueryValidator()
    {
        PagedQueryRules.AddPagingRules(this, VehicleService.SortableFields.Keys);
        RuleFor(x => x.MinSeats).InclusiveBetween(1, 60).When(x => x.MinSeats.HasValue);
    }
}
