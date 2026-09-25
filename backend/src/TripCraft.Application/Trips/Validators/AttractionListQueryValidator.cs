using FluentValidation;
using TripCraft.Application.Trips.Dtos;
using TripCraft.Application.Trips.Services;

namespace TripCraft.Application.Trips.Validators;

public class AttractionListQueryValidator : AbstractValidator<AttractionListQuery>
{
    public AttractionListQueryValidator()
    {
        PagedQueryRules.AddPagingRules(this, AttractionService.SortableFields.Keys);
        RuleFor(x => x.City).MaximumLength(100);
        RuleFor(x => x.Category).MaximumLength(50);
    }
}
