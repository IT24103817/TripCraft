using FluentValidation;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Resources.Dtos;
using TripCraft.Application.Resources.Services;

namespace TripCraft.Application.Resources.Validators;

public class GuideDetailsValidator : AbstractValidator<IGuideDetails>
{
    public GuideDetailsValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Length(2, 100);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(30)
            .Matches(@"^\+?[0-9 ]{7,20}$").WithMessage("Phone must be 7–20 digits, optionally starting with +.");
        RuleFor(x => x.Languages).NotEmpty().WithMessage("A guide speaks at least one language.")
            .Must(l => l.Count <= 8).WithMessage("At most 8 languages.")
            .Must(l => l.Distinct(StringComparer.OrdinalIgnoreCase).Count() == l.Count)
            .WithMessage("Languages must not repeat.");
        RuleForEach(x => x.Languages).Matches("^[a-zA-Z]{2}$").WithMessage("Use two-letter language codes, e.g. en.");
        RuleFor(x => x.DayRateLkr).GreaterThan(0).LessThanOrEqualTo(1_000_000);
        RuleFor(x => x.MaxPax).InclusiveBetween(1, 50);
    }
}

public class SaveGuideRequestValidator : AbstractValidator<SaveGuideRequest>
{
    public SaveGuideRequestValidator() => Include(new GuideDetailsValidator());
}

public class CreateGuideRequestValidator : AbstractValidator<CreateGuideRequest>
{
    public CreateGuideRequestValidator()
    {
        Include(new GuideDetailsValidator());
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
    }
}

public class GuideListQueryValidator : AbstractValidator<GuideListQuery>
{
    public GuideListQueryValidator()
    {
        PagedQueryRules.AddPagingRules(this, GuideService.SortableFields.Keys);
        RuleFor(x => x.Language).Matches("^[a-zA-Z]{2}$").When(x => !string.IsNullOrEmpty(x.Language));
    }
}
