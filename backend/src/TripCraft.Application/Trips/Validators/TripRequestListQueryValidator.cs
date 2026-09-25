using FluentValidation;
using TripCraft.Application.Trips.Dtos;
using TripCraft.Application.Trips.Services;

namespace TripCraft.Application.Trips.Validators;

public class TripRequestListQueryValidator : AbstractValidator<TripRequestListQuery>
{
    public TripRequestListQueryValidator()
    {
        PagedQueryRules.AddPagingRules(this, TripRequestService.SortableFields.Keys);
        RuleFor(x => x.To).GreaterThanOrEqualTo(x => x.From)
            .When(x => x.From.HasValue && x.To.HasValue)
            .WithMessage("'to' must be on or after 'from'.");
    }
}
