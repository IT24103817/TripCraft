using FluentAssertions;
using FluentValidation.TestHelper;
using TripCraft.Application.Resources.Dtos;
using TripCraft.Application.Resources.Validators;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Tests.Resources;

/// <summary>Unit tests of Component B's FluentValidation rules (the endpoints only show that a 400 comes back).</summary>
public class ResourceValidatorsTests
{
    private static readonly DateOnly Day = new(2026, 11, 1);

    [Fact]
    public void A_guide_needs_a_real_phone_two_letter_unique_languages_and_a_positive_rate()
    {
        var validator = new SaveGuideRequestValidator();
        var good = new SaveGuideRequest("Nimal Perera", "+94 77 123 4567", ["en", "si"], 6000, 10, true);

        validator.TestValidate(good).ShouldNotHaveAnyValidationErrors();
        validator.TestValidate(good with { Phone = "call me" }).ShouldHaveValidationErrorFor(x => x.Phone);
        validator.TestValidate(good with { Languages = [] }).ShouldHaveValidationErrorFor(x => x.Languages);
        validator.TestValidate(good with { Languages = ["en", "EN"] }).ShouldHaveValidationErrorFor(x => x.Languages)
            .WithErrorMessage("Languages must not repeat.");
        validator.TestValidate(good with { Languages = ["english"] }).ShouldHaveValidationErrorFor("Languages[0]");
        validator.TestValidate(good with { DayRateLkr = 0 }).ShouldHaveValidationErrorFor(x => x.DayRateLkr);
        validator.TestValidate(good with { MaxPax = 51 }).ShouldHaveValidationErrorFor(x => x.MaxPax);
    }

    [Fact]
    public void A_vehicle_has_a_known_type_a_clean_registration_and_1_to_60_seats()
    {
        var validator = new SaveVehicleRequestValidator();
        var good = new SaveVehicleRequest("CAB-1234", "Van", 6, 120, true);

        validator.TestValidate(good).ShouldNotHaveAnyValidationErrors();
        validator.TestValidate(good with { Type = "Tuk-tuk" }).ShouldHaveValidationErrorFor(x => x.Type);
        validator.TestValidate(good with { RegistrationNo = "CAB 1234!" }).ShouldHaveValidationErrorFor(x => x.RegistrationNo);
        validator.TestValidate(good with { Seats = 0 }).ShouldHaveValidationErrorFor(x => x.Seats);
    }

    [Fact]
    public void A_hotel_is_in_sri_lanka_with_1_to_5_stars_and_room_types_sleep_1_to_8()
    {
        var hotels = new SaveHotelRequestValidator();
        var hotel = new SaveHotelRequest("Kandy Hills", "Kandy", 4, 7.29, 80.63, true);
        var rooms = new SaveRoomTypeRequestValidator();

        hotels.TestValidate(hotel).ShouldNotHaveAnyValidationErrors();
        hotels.TestValidate(hotel with { Latitude = 48.85, Longitude = 2.35 })
            .ShouldHaveValidationErrorFor(x => x.Latitude).WithErrorMessage("Latitude must be in Sri Lanka (5.5–10.0).");
        hotels.TestValidate(hotel with { StarRating = 6 }).ShouldHaveValidationErrorFor(x => x.StarRating);
        rooms.TestValidate(new SaveRoomTypeRequest("Family Room", 4, 20000, 3)).ShouldNotHaveAnyValidationErrors();
        rooms.TestValidate(new SaveRoomTypeRequest("Dorm", 9, 5000, 1)).ShouldHaveValidationErrorFor(x => x.Capacity);
    }

    [Fact]
    public void Availability_asks_for_what_each_resource_type_needs_within_60_days()
    {
        var validator = new AvailabilityQueryValidator();

        validator.TestValidate(new AvailabilityQuery { Type = ResourceType.Guide, From = Day, To = Day.AddDays(4), Language = "en", Pax = 4 })
            .ShouldNotHaveAnyValidationErrors();
        validator.TestValidate(new AvailabilityQuery { Type = ResourceType.Guide, From = Day, To = Day, Pax = 4 })
            .ShouldHaveValidationErrorFor(x => x.Language);
        validator.TestValidate(new AvailabilityQuery { Type = ResourceType.Vehicle, From = Day, To = Day })
            .ShouldHaveValidationErrorFor(x => x.Seats);
        validator.TestValidate(new AvailabilityQuery { Type = ResourceType.Room, From = Day, To = Day, Rooms = 2 })
            .ShouldHaveValidationErrorFor(x => x.City);
        validator.TestValidate(new AvailabilityQuery { Type = ResourceType.Vehicle, From = Day, To = Day.AddDays(-1), Seats = 4 })
            .ShouldHaveValidationErrorFor(x => x.To);
        validator.TestValidate(new AvailabilityQuery { Type = ResourceType.Vehicle, From = Day, To = Day.AddDays(61), Seats = 4 })
            .Errors.Should().Contain(e => e.ErrorMessage == "Ask for at most 60 days at a time.");
    }

    [Fact]
    public void Guides_and_vehicles_are_held_one_at_a_time_and_check_ins_need_real_coordinates()
    {
        var holds = new CreateHoldRequestValidator();
        var checkIns = new CheckInRequestValidator();

        holds.TestValidate(new CreateHoldRequest(ResourceType.Room, Guid.NewGuid(), Day, Day, 3, "Wedding block"))
            .ShouldNotHaveAnyValidationErrors();
        holds.TestValidate(new CreateHoldRequest(ResourceType.Guide, Guid.NewGuid(), Day, Day, 2, null))
            .ShouldHaveValidationErrorFor(x => x.Quantity).WithErrorMessage("Guides and vehicles are held one at a time.");
        holds.TestValidate(new CreateHoldRequest(ResourceType.Vehicle, Guid.Empty, Day, Day.AddDays(-1), 1, null))
            .ShouldHaveValidationErrorFor(x => x.ResourceId);
        checkIns.TestValidate(new CheckInRequest(Guid.NewGuid(), 7.29, 80.64)).ShouldNotHaveAnyValidationErrors();
        checkIns.TestValidate(new CheckInRequest(Guid.Empty, 95, 200)).Errors.Should().HaveCount(3);
    }
}
