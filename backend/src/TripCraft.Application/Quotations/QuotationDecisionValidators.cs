using FluentValidation;

namespace TripCraft.Application.Quotations;

public class QuotationDecisionRequestValidator : AbstractValidator<QuotationDecisionRequest>
{
    public QuotationDecisionRequestValidator() => RuleFor(r => r.Comment).MaximumLength(1000);
}

public class DeclineQuotationRequestValidator : AbstractValidator<DeclineQuotationRequest>
{
    public DeclineQuotationRequestValidator() => RuleFor(r => r.Reason).NotEmpty().MaximumLength(1000);
}

public class ReplanRequestValidator : AbstractValidator<ReplanRequest>
{
    public ReplanRequestValidator() => RuleFor(r => r.Note).NotEmpty().MaximumLength(1000);
}
