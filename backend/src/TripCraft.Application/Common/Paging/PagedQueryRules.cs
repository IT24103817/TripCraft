using FluentValidation;

namespace TripCraft.Application.Common.Paging;

public static class PagedQueryRules
{
    public static void AddPagingRules<T>(AbstractValidator<T> v, IEnumerable<string> sortableFields) where T : PagedQuery
    {
        var allowed = sortableFields.ToList();
        v.RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        v.RuleFor(x => x.PageSize).InclusiveBetween(1, PagedQuery.MaxPageSize);
        v.RuleFor(x => x.Search).MaximumLength(100);
        v.RuleFor(x => x.Sort)
            .Must(s => QueryableExtensions.IsValidSort(s, allowed))
            .WithMessage($"Sort must be one of: {string.Join(", ", allowed)} (prefix with '-' for descending).");
    }
}
