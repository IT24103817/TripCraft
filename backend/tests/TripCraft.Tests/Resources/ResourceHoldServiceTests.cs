using FluentAssertions;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Resources;
using TripCraft.Application.Resources.Services;
using TripCraft.Application.Workflows.Ports;

namespace TripCraft.Tests.Resources;

/// <summary>Component B business operation: the hold overlap check that the approval transaction relies on.</summary>
public class ResourceHoldServiceTests : IDisposable
{
    private static readonly DateOnly Start = new(2026, 10, 10);
    private readonly ResourceTestDb _t = new();

    private ResourceHoldService Service() => new(_t.Repository);

    private static ResourceHoldRequest Request(ResourceType type, Guid id, int fromOffset, int toOffset, int quantity = 1) =>
        new(type, id, Guid.NewGuid(), Start.AddDays(fromOffset), Start.AddDays(toOffset), quantity);

    [Fact]
    public async Task A_free_guide_is_staged_as_a_Held_hold_but_not_saved()
    {
        await Service().CreateHoldAsync(Request(ResourceType.Guide, ResourceTestDb.EnglishGuide, 0, 4), CancellationToken.None);

        var staged = _t.Repository.StagedHolds().Should().ContainSingle().Subject;
        staged.Status.Should().Be(HoldStatus.Held);
        _t.Db.ResourceHolds.Count().Should().Be(0, "the caller's transaction commits it");
    }

    [Fact]
    public async Task An_overlapping_guide_hold_is_a_conflict()
    {
        _t.Hold(ResourceType.Guide, ResourceTestDb.EnglishGuide, Start.AddDays(3), Start.AddDays(6));

        var act = () => Service().CreateHoldAsync(Request(ResourceType.Guide, ResourceTestDb.EnglishGuide, 0, 4), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("Guide is already held*");
    }

    [Fact]
    public async Task A_released_hold_no_longer_blocks_the_vehicle()
    {
        _t.Hold(ResourceType.Vehicle, ResourceTestDb.Van, Start, Start.AddDays(4), status: HoldStatus.Released);

        await Service().CreateHoldAsync(Request(ResourceType.Vehicle, ResourceTestDb.Van, 0, 4), CancellationToken.None);

        _t.Repository.StagedHolds().Should().ContainSingle();
    }

    [Fact]
    public async Task Rooms_can_be_held_up_to_the_total_and_one_more_is_a_conflict()
    {
        _t.Hold(ResourceType.Room, ResourceTestDb.KandyDouble, Start, Start, quantity: 2);

        await Service().CreateHoldAsync(Request(ResourceType.Room, ResourceTestDb.KandyDouble, 0, 0, quantity: 1), CancellationToken.None);
        var act = () => Service().CreateHoldAsync(Request(ResourceType.Room, ResourceTestDb.KandyDouble, 0, 0, quantity: 1), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("Only 0 Double rooms are free on 2026-10-10.");
    }

    [Fact]
    public async Task Unknown_or_inactive_resources_are_refused()
    {
        var act = () => Service().CreateHoldAsync(Request(ResourceType.Vehicle, Guid.NewGuid(), 0, 1), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("*does not exist or is inactive*");
    }

    public void Dispose() => _t.Dispose();
}
