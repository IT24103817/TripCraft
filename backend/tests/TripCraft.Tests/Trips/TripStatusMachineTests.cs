using FluentAssertions;
using Moq;
using TripCraft.Application.Common.Auditing;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Trips;
using static TripCraft.Application.Trips.TripRequestStatus;

namespace TripCraft.Tests.Trips;

/// <summary>The v1.1 trip lifecycle: every allowed move, a sample of illegal ones, and what Move records.</summary>
public class TripStatusMachineTests
{
    public static TheoryData<TripRequestStatus, TripRequestStatus> AllowedMoves => new()
    {
        { Submitted, Planning }, { Submitted, Cancelled },
        { Planning, PendingReview }, { Planning, FailedSafely },
        { FailedSafely, Planning }, { FailedSafely, Cancelled },
        { PendingReview, QuotationSent }, { PendingReview, RevisionRequested }, { PendingReview, Cancelled },
        { RevisionRequested, PendingReview }, { RevisionRequested, FailedSafely }, { RevisionRequested, Cancelled },
        { QuotationSent, ClientAccepted }, { QuotationSent, PendingReview }, { QuotationSent, Cancelled },
        { ClientAccepted, Confirmed }, { ClientAccepted, PendingReview }, { ClientAccepted, Cancelled },
        { Confirmed, InProgress }, { Confirmed, Cancelled },
        { InProgress, Completed }
    };

    [Theory]
    [MemberData(nameof(AllowedMoves))]
    public void Allowed_moves_are_allowed(TripRequestStatus from, TripRequestStatus to)
    {
        TripStatusMachine.CanMove(from, to).Should().BeTrue();
    }

    [Fact]
    public void Exactly_the_documented_moves_are_allowed_and_nothing_else()
    {
        var documented = AllowedMoves.Select(row => ((TripRequestStatus)row[0], (TripRequestStatus)row[1])).ToHashSet();
        var all = Enum.GetValues<TripRequestStatus>();

        foreach (var from in all)
            foreach (var to in all)
                TripStatusMachine.CanMove(from, to).Should().Be(documented.Contains((from, to)), $"{from} → {to}");
    }

    [Theory]
    [InlineData(Submitted, Confirmed)]        // no skipping the review
    [InlineData(PendingReview, Confirmed)]    // the client must accept first
    [InlineData(QuotationSent, Confirmed)]
    [InlineData(Planning, Cancelled)]         // not while the agents run
    [InlineData(InProgress, Cancelled)]       // a running tour is not cancelled in the app
    [InlineData(Completed, Cancelled)]
    [InlineData(Cancelled, Submitted)]
    public void Illegal_moves_are_409_with_a_readable_message(TripRequestStatus from, TripRequestStatus to)
    {
        var act = () => TripStatusMachine.EnsureCanMove(from, to);

        act.Should().Throw<ConflictException>()
            .WithMessage($"A trip that is {TripStatusMachine.Describe(from)} cannot become {TripStatusMachine.Describe(to)}.");
    }

    [Fact]
    public void Completed_and_Cancelled_are_terminal()
    {
        TripStatusMachine.NextOf(Completed).Should().BeEmpty();
        TripStatusMachine.NextOf(Cancelled).Should().BeEmpty();
    }

    [Fact]
    public void Move_changes_the_status_and_records_actor_and_reason_in_the_history()
    {
        var audit = new Mock<IAuditLogger>();
        var trip = new TripRequest { Status = QuotationSent };
        var actor = Guid.NewGuid();

        TripStatusMachine.Move(trip, ClientAccepted, actor, "The client accepted quotation v1.", audit.Object);

        trip.Status.Should().Be(ClientAccepted);
        audit.Verify(a => a.Record(actor, "TripRequestStatusChanged", nameof(TripRequest), trip.Id,
            It.Is<object>(o => o.ToString()!.Contains("QuotationSent")),
            It.Is<object>(o => o.ToString()!.Contains("ClientAccepted") && o.ToString()!.Contains("The client accepted quotation v1."))),
            Times.Once);
    }

    [Fact]
    public void An_illegal_move_changes_nothing_and_records_nothing()
    {
        var audit = new Mock<IAuditLogger>();
        var trip = new TripRequest { Status = Submitted };

        var act = () => TripStatusMachine.Move(trip, Confirmed, null, "skip", audit.Object);

        act.Should().Throw<ConflictException>();
        trip.Status.Should().Be(Submitted);
        audit.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(PendingReview, "pending review")]
    [InlineData(FailedSafely, "failed safely")]
    [InlineData(Submitted, "submitted")]
    public void Describe_turns_the_name_into_words(TripRequestStatus status, string expected)
    {
        TripStatusMachine.Describe(status).Should().Be(expected);
    }
}
