using FluentValidation;
using TripCraft.Application.Common.Paging;
using TripCraft.Application.Resources.Dtos;
using TripCraft.Application.Resources.Services;

namespace TripCraft.Application.Resources.Validators;

public class SaveHotelRequestValidator : AbstractValidator<SaveHotelRequest>
{
    public SaveHotelRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Length(2, 150);
        RuleFor(x => x.City).NotEmpty().Length(2, 100);
        RuleFor(x => x.StarRating).InclusiveBetween(1, 5);
        // Sri Lanka's bounding box, so a typo in the coordinates is caught.
        RuleFor(x => x.Latitude).InclusiveBetween(5.5, 10.0).WithMessage("Latitude must be in Sri Lanka (5.5–10.0).");
        RuleFor(x => x.Longitude).InclusiveBetween(79.4, 82.1).WithMessage("Longitude must be in Sri Lanka (79.4–82.1).");
        RuleFor(x => x.RoomTypes).NotEmpty().WithMessage("Add at least one room type.");
        RuleFor(x => x.RoomTypes)
            .Must(rows => rows.Select(r => r.Name.Trim().ToLowerInvariant()).Distinct().Count() == rows.Count)
            .When(x => x.RoomTypes is { Count: > 0 }).WithMessage("Room type names must be unique within the hotel.");
        RuleForEach(x => x.RoomTypes).ChildRules(row =>
        {
            row.RuleFor(r => r.Name).NotEmpty().Length(2, 60);
            row.RuleFor(r => r.Capacity).InclusiveBetween(1, 8);
            row.RuleFor(r => r.RatePerNightLkr).GreaterThan(0).LessThanOrEqualTo(1_000_000);
            row.RuleFor(r => r.TotalRooms).InclusiveBetween(1, 500);
        });
    }
}

public class SaveRoomTypeRequestValidator : AbstractValidator<SaveRoomTypeRequest>
{
    public SaveRoomTypeRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Length(2, 60);
        RuleFor(x => x.Capacity).InclusiveBetween(1, 8);
        RuleFor(x => x.RatePerNightLkr).GreaterThan(0).LessThanOrEqualTo(1_000_000);
        RuleFor(x => x.TotalRooms).InclusiveBetween(1, 500);
    }
}

public class HotelListQueryValidator : AbstractValidator<HotelListQuery>
{
    public HotelListQueryValidator()
    {
        PagedQueryRules.AddPagingRules(this, HotelService.SortableFields.Keys);
        RuleFor(x => x.MinStars).InclusiveBetween(1, 5).When(x => x.MinStars.HasValue);
    }
}
