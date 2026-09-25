using FluentValidation;
using TripCraft.Application.Workflows.Dtos;

namespace TripCraft.Application.Workflows.Validation;

public class WorkflowListQueryValidator : AbstractValidator<WorkflowListQuery>
{
    public WorkflowListQueryValidator()
    {
        RuleFor(q => q.Page).GreaterThanOrEqualTo(1);
        RuleFor(q => q.PageSize).InclusiveBetween(1, WorkflowListQuery.MaxPageSize);
    }
}
