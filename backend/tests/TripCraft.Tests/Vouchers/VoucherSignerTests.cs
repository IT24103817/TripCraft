using FluentAssertions;
using TripCraft.Application.Trips;
using TripCraft.Application.Vouchers;
using TripCraft.Application.Workflows;
using TripCraft.Tests.Workflows;

namespace TripCraft.Tests.Vouchers;

/// <summary>HMAC-signed voucher codes: what verifies and what is rejected.</summary>
public class VoucherSignerTests
{
    private readonly VoucherSigner _signer = new("unit-test-voucher-signing-key-0123456789");

    [Fact]
    public void A_signed_code_verifies_with_or_without_the_QR_prefix_and_gives_back_its_claims()
    {
        var voucherId = Guid.NewGuid();
        var tripId = Guid.NewGuid();
        var night = new DateOnly(2026, 11, 2);

        var code = _signer.Sign(voucherId, tripId, VoucherType.HotelNight, night);

        _signer.Verify(code).Should().Be(new VoucherClaims(voucherId, tripId, VoucherType.HotelNight, night));
        _signer.Verify(VoucherSigner.QrPrefix + code).Should().NotBeNull();
        _signer.Verify(_signer.Sign(voucherId, tripId, VoucherType.Trip, null))!.Night.Should().BeNull();
    }

    [Fact]
    public void A_changed_trip_type_night_or_signature_is_rejected()
    {
        var code = _signer.Sign(Guid.NewGuid(), Guid.NewGuid(), VoucherType.Trip, null);
        var parts = code.Split('.');

        string With(int index, string value) { var copy = (string[])parts.Clone(); copy[index] = value; return string.Join('.', copy); }

        _signer.Verify(With(2, Guid.NewGuid().ToString("N"))).Should().BeNull("another trip");
        _signer.Verify(With(3, "H")).Should().BeNull("another type");
        _signer.Verify(With(4, "20261102")).Should().BeNull("a night added");
        _signer.Verify(With(5, new string('A', 22))).Should().BeNull("a forged signature");
        new VoucherSigner("another-key-that-is-also-long-enough-32b").Verify(code).Should().BeNull("another key");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("TRIPCRAFT-HOTEL:abc")]
    [InlineData("1.not.enough")]
    public void Garbage_is_rejected(string? code)
    {
        _signer.Verify(code).Should().BeNull();
    }

    [Fact]
    public void A_missing_or_short_key_is_refused()
    {
        var act = () => new VoucherSigner("too-short");

        act.Should().Throw<InvalidOperationException>().WithMessage("VOUCHER_SIGNING_KEY*");
    }

    [Fact]
    public void Builder_makes_one_trip_voucher_and_one_per_hotel_night_with_the_room_count()
    {
        var start = new DateOnly(2026, 10, 10);
        var proposal = TestProposals.Golden(start, DemoAttractions.Random);
        var trip = new TripRequest { StartDate = start, EndDate = start.AddDays(4) };

        var vouchers = VoucherBuilder.Build(trip,
            new StoredProposal(proposal.Days, proposal.Resources, proposal.Quotation, [], 0, null), _signer);

        vouchers.Should().HaveCount(5);
        vouchers.Should().ContainSingle(v => v.Type == VoucherType.Trip && v.Night == null);
        vouchers.Where(v => v.Type == VoucherType.HotelNight).Select(v => (v.Night, v.Rooms))
            .Should().Equal((start, 2), (start.AddDays(1), 2), (start.AddDays(2), 2), (start.AddDays(3), 2));
        vouchers.Should().OnlyContain(v => _signer.Verify(v.QrPayload)!.VoucherId == v.Id);
    }
}
