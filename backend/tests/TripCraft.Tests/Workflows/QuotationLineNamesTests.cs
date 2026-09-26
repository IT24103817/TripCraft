using FluentAssertions;
using TripCraft.Application.Workflows;
using TripCraft.Application.Workflows.Dtos;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Tests.Workflows;

/// <summary>The tourist's quotation lines show names, not ids (found in the section 6 run from the emulator).</summary>
public class QuotationLineNamesTests
{
    private static readonly Guid RoomTypeId = Guid.NewGuid();

    private static readonly ProposalFacts Facts = new(
        new Dictionary<Guid, decimal>(),
        new GuideOption(Guid.NewGuid(), "Ruwan Fernando", ["en"], 12),
        new VehicleOption(Guid.NewGuid(), "NC-4455", "Coach", 15),
        new Dictionary<Guid, RoomOption> { [RoomTypeId] = new(Guid.NewGuid(), "Ella Gap", RoomTypeId, "Standard Double", 2, 0) },
        false, false, null);

    [Theory]
    [InlineData("guide", "Guide 00000000-0000-0000-0000-00000000a003", "Guide Ruwan Fernando")]
    [InlineData("vehicle", "Vehicle 00000000-0000-0000-0000-00000000b003", "Coach NC-4455")]
    [InlineData("entry", "Nine Arches Bridge (day 3)", "Nine Arches Bridge (day 3)")]
    public void Guide_and_vehicle_lines_get_names_and_other_lines_are_kept(string type, string description, string expected)
    {
        QuotationLineNames.Describe(new ProposalQuotationLine(type, description, 1, 1, 1), Facts).Should().Be(expected);
    }

    [Fact]
    public void A_room_line_gets_the_hotel_and_room_type_names_and_an_unknown_one_is_kept()
    {
        QuotationLineNames.Describe(new ProposalQuotationLine("room", $"Room type {RoomTypeId}", 6, 12000, 72000), Facts)
            .Should().Be("Ella Gap — Standard Double");
        QuotationLineNames.Describe(new ProposalQuotationLine("room", "Room type x", 1, 1, 1), Facts)
            .Should().Be("Room type x");
    }
}
