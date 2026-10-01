using FluentValidation;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Application.Resources.Dtos;

/// <summary>GET /api/availability/grid?from=&amp;to=&amp;type=&amp;language=&amp;seats=&amp;city= (at most 62 days).</summary>
public class AvailabilityGridQuery
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public ResourceType? Type { get; set; }
    public string? Language { get; set; }
    public int? Seats { get; set; }
    public string? City { get; set; }
}

/// <summary>Free / Held (trip not confirmed yet) / Confirmed (trip booked) / Blocked (manual: leave, maintenance).</summary>
public record AvailabilityCellDto(DateOnly Date, string State, Guid? HoldId, Guid? TripRequestId, string? TouristName,
    string? TripStatus, string? Note, int HeldQuantity, int FreeQuantity);

/// <summary>One resource row. Capacity is 1 for guides and vehicles, the total rooms for a room type.</summary>
public record AvailabilityRowDto(ResourceType ResourceType, Guid ResourceId, string Name, string Detail, int Capacity,
    IReadOnlyList<AvailabilityCellDto> Cells);

public record AvailabilityGridDto(IReadOnlyList<DateOnly> Days, IReadOnlyList<AvailabilityRowDto> Rows);

/// <summary>PUT /api/resource-holds/{id}: change a manual block (not a trip's hold).</summary>
public record UpdateBlockRequest(DateOnly FromDate, DateOnly ToDate, int Quantity, string? Note);

public class AvailabilityGridQueryValidator : AbstractValidator<AvailabilityGridQuery>
{
    public const int MaxDays = 62;

    public AvailabilityGridQueryValidator()
    {
        RuleFor(x => x.From).NotEmpty();
        RuleFor(x => x.To).GreaterThanOrEqualTo(x => x.From).WithMessage("'to' must be on or after 'from'.");
        RuleFor(x => x).Must(x => x.To.DayNumber - x.From.DayNumber < MaxDays)
            .WithMessage($"The grid shows at most {MaxDays} days.").WithName("to");
        RuleFor(x => x.Type).IsInEnum().When(x => x.Type.HasValue);
        RuleFor(x => x.Language).Matches("^[a-zA-Z]{2}$").When(x => !string.IsNullOrEmpty(x.Language));
        RuleFor(x => x.Seats).InclusiveBetween(1, 60).When(x => x.Seats.HasValue);
        RuleFor(x => x.City).MaximumLength(100);
    }
}

public class UpdateBlockRequestValidator : AbstractValidator<UpdateBlockRequest>
{
    public UpdateBlockRequestValidator()
    {
        RuleFor(x => x.ToDate).GreaterThanOrEqualTo(x => x.FromDate).WithMessage("The block must end on or after it starts.");
        RuleFor(x => x.Quantity).InclusiveBetween(1, 500);
        RuleFor(x => x.Note).MaximumLength(500);
    }
}
