using FluentValidation;

namespace TripCraft.Application.Identity.Validators;

public static class PasswordRules
{
    /// <summary>At least 8 characters with an upper-case letter, a lower-case letter and a digit.</summary>
    public static IRuleBuilderOptions<T, string> StrongPassword<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain an upper-case letter.")
            .Matches("[a-z]").WithMessage("Password must contain a lower-case letter.")
            .Matches("[0-9]").WithMessage("Password must contain a digit.");
}
