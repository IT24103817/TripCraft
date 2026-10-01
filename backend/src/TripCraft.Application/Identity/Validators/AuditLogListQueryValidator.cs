using FluentValidation;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Identity.Dtos;
using TripCraft.Application.Identity.Services;

namespace TripCraft.Application.Identity.Validators;

public class AuditLogListQueryValidator : AbstractValidator<AuditLogListQuery>
{
    public AuditLogListQueryValidator()
    {
        PagedQueryRules.AddPagingRules(this, AuditLogQueryService.SortableFields.Keys);
        RuleFor(x => x.Actor).MaximumLength(256);
        RuleFor(x => x.Entity).MaximumLength(100);
        RuleFor(x => x.Action).MaximumLength(100);
        RuleFor(x => x.To).GreaterThanOrEqualTo(x => x.From)
            .When(x => x.From.HasValue && x.To.HasValue)
            .WithMessage("'to' must be on or after 'from'.");
    }
}
