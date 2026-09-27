using FluentValidation;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Workflows.Dtos;
using TripCraft.Application.Workflows.Services;

namespace TripCraft.Application.Workflows.Validation;

public class WorkflowListQueryValidator : AbstractValidator<WorkflowListQuery>
{
    public WorkflowListQueryValidator()
    {
        PagedQueryRules.AddPagingRules(this, WorkflowQueryService.SortableFields.Keys);
    }
}
