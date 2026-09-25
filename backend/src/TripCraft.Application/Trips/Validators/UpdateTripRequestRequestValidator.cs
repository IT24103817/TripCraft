using FluentValidation;
using TripCraft.Application.Trips.Dtos;

namespace TripCraft.Application.Trips.Validators;

public class UpdateTripRequestRequestValidator : AbstractValidator<UpdateTripRequestRequest>
{
    public UpdateTripRequestRequestValidator()
    {
        Include(new TripDetailsValidator());
    }
}
