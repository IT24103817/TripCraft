using FluentValidation;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Resources.Dtos;
using TripCraft.Application.Resources.Services;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Application.Resources.Validators;

public class AvailabilityQueryValidator : AbstractValidator<AvailabilityQuery>
{
    public AvailabilityQueryValidator()
    {
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.From).NotEmpty();
        RuleFor(x => x.To).GreaterThanOrEqualTo(x => x.From).WithMessage("'to' must be on or after 'from'.");
        RuleFor(x => x).Must(x => x.To.DayNumber - x.From.DayNumber <= 60)
            .WithMessage("Ask for at most 60 days at a time.").WithName("to");
        RuleFor(x => x.Language).NotEmpty().Matches("^[a-zA-Z]{2}$").When(x => x.Type == ResourceType.Guide);
        RuleFor(x => x.Pax).NotNull().InclusiveBetween(1, 50).When(x => x.Type == ResourceType.Guide);
        RuleFor(x => x.Seats).NotNull().InclusiveBetween(1, 60).When(x => x.Type == ResourceType.Vehicle);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100).When(x => x.Type == ResourceType.Room);
        RuleFor(x => x.Rooms).NotNull().InclusiveBetween(1, 50).When(x => x.Type == ResourceType.Room);
    }
}

public class CreateHoldRequestValidator : AbstractValidator<CreateHoldRequest>
{
    public CreateHoldRequestValidator()
    {
        RuleFor(x => x.ResourceType).IsInEnum();
        RuleFor(x => x.ResourceId).NotEmpty();
        RuleFor(x => x.ToDate).GreaterThanOrEqualTo(x => x.FromDate).WithMessage("'toDate' must be on or after 'fromDate'.");
        RuleFor(x => x.Quantity).Equal(1).When(x => x.ResourceType != ResourceType.Room)
            .WithMessage("Guides and vehicles are held one at a time.");
        RuleFor(x => x.Quantity).InclusiveBetween(1, 50);
        RuleFor(x => x.Note).MaximumLength(300);
    }
}

public class HoldListQueryValidator : AbstractValidator<HoldListQuery>
{
    public HoldListQueryValidator()
    {
        PagedQueryRules.AddPagingRules(this, ResourceHoldAdminService.SortableFields.Keys);
        RuleFor(x => x.To).GreaterThanOrEqualTo(x => x.From).When(x => x.From.HasValue && x.To.HasValue)
            .WithMessage("'to' must be on or after 'from'.");
    }
}

public class CheckInRequestValidator : AbstractValidator<CheckInRequest>
{
    public CheckInRequestValidator()
    {
        RuleFor(x => x.ItineraryStopId).NotEmpty();
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180);
    }
}
