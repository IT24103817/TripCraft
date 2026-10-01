using FluentValidation;

namespace TripCraft.Application.Quotations;

public class QuotationDecisionRequestValidator : AbstractValidator<QuotationDecisionRequest>
{
    public QuotationDecisionRequestValidator() => RuleFor(r => r.Comment).MaximumLength(1000);
}

public class RequestRevisionRequestValidator : AbstractValidator<RequestRevisionRequest>
{
    public RequestRevisionRequestValidator() => RuleFor(r => r.Comment).NotEmpty().MaximumLength(1000);
}

public class DeclineQuotationRequestValidator : AbstractValidator<DeclineQuotationRequest>
{
    public DeclineQuotationRequestValidator() => RuleFor(r => r.Reason).NotEmpty().MaximumLength(1000);
}
