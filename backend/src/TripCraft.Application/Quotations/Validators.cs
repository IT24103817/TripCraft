using FluentValidation;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Quotations.Dtos;
using TripCraft.Application.Quotations.Reports;
using TripCraft.Application.Quotations.Services;

namespace TripCraft.Application.Quotations;

public class QuotationListQueryValidator : AbstractValidator<QuotationListQuery>
{
    public QuotationListQueryValidator()
    {
        PagedQueryRules.AddPagingRules(this, QuotationService.SortableFields.Keys);
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.To).GreaterThanOrEqualTo(x => x.From).When(x => x.From.HasValue && x.To.HasValue)
            .WithMessage("'to' must be on or after 'from'.");
        RuleFor(x => x.MinTotalUsd).GreaterThanOrEqualTo(0).When(x => x.MinTotalUsd.HasValue);
    }
}

public class ReportRangeQueryValidator : AbstractValidator<ReportRangeQuery>
{
    public ReportRangeQueryValidator()
    {
        RuleFor(x => x.From).NotEmpty();
        RuleFor(x => x.To).GreaterThanOrEqualTo(x => x.From).WithMessage("'to' must be on or after 'from'.");
        RuleFor(x => x).Must(x => x.To.DayNumber - x.From.DayNumber <= 366)
            .WithMessage("Reports cover at most one year.").WithName("to");
    }
}

public class SetPaymentRequestValidator : AbstractValidator<SetPaymentRequest>
{
    public SetPaymentRequestValidator() => RuleFor(x => x.Paid).NotNull();
}
