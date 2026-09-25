using FluentValidation;
using TripCraft.Application.Trips.Dtos;

namespace TripCraft.Application.Trips.Validators;

public class CreateTripRequestRequestValidator : AbstractValidator<CreateTripRequestRequest>
{
    public CreateTripRequestRequestValidator()
    {
        Include(new TripDetailsValidator());

        RuleFor(x => x.Nationality).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PassportNumber).NotEmpty()
            .Matches("^[A-Za-z0-9 ]{6,20}$").WithMessage("Passport number must be 6–20 letters or digits.");
    }
}
