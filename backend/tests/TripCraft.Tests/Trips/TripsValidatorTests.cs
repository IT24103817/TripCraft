using System.Text.Json;
using FluentAssertions;
using TripCraft.Application.Trips.Dtos;
using TripCraft.Application.Trips.Validators;

namespace TripCraft.Tests.Trips;

public class TripsValidatorTests
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);
    private readonly CreateTripRequestRequestValidator _validator = new();

    private static CreateTripRequestRequest Valid() => new(
        "5 days in Kandy and Ella", Today.AddDays(7), Today.AddDays(11), 4, 1500,
        JsonDocument.Parse("""{"language":"en"}""").RootElement, "United Kingdom", "N1234567");

    [Fact]
    public void Valid_request_passes()
    {
        _validator.Validate(Valid()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Start_today_and_thirty_day_trip_are_the_allowed_boundaries()
    {
        var request = Valid() with { StartDate = Today, EndDate = Today.AddDays(29) };

        _validator.Validate(request).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(-1, 3, "StartDate")]   // starts yesterday
    [InlineData(5, 4, "EndDate")]      // ends before it starts
    [InlineData(0, 30, "EndDate")]     // 31 days
    public void Bad_dates_are_rejected_on_the_right_field(int startOffset, int endOffset, string field)
    {
        var request = Valid() with { StartDate = Today.AddDays(startOffset), EndDate = Today.AddDays(endOffset) };

        var result = _validator.Validate(request);

        result.Errors.Should().Contain(e => e.PropertyName == field);
    }

    [Theory]
    [InlineData("12345")]            // too short
    [InlineData("N1234567!")]        // symbol
    public void Bad_passport_number_is_rejected(string passport)
    {
        var result = _validator.Validate(Valid() with { PassportNumber = passport });

        result.Errors.Should().ContainSingle(e => e.PropertyName == "PassportNumber");
    }

    [Fact]
    public void Zero_pax_zero_budget_and_array_preferences_are_all_reported()
    {
        var request = Valid() with
        {
            Pax = 0, BudgetUsd = 0, Preferences = JsonDocument.Parse("[1,2]").RootElement
        };

        var fields = _validator.Validate(request).Errors.Select(e => e.PropertyName).ToList();

        fields.Should().Contain(["Pax", "BudgetUsd", "Preferences"]);
    }
}
