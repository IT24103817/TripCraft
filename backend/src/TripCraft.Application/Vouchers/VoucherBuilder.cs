using TripCraft.Application.Trips;
using TripCraft.Application.Workflows;

namespace TripCraft.Application.Vouchers;

/// <summary>
/// The vouchers of a confirmed trip (pure, so it is unit tested): one Trip voucher, and one HotelNight voucher per
/// hotel and night with the number of rooms booked that night.
/// </summary>
public static class VoucherBuilder
{
    public static List<Voucher> Build(TripRequest trip, StoredProposal proposal, VoucherSigner signer)
    {
        var vouchers = new List<Voucher> { Signed(new Voucher { TripRequestId = trip.Id, Type = VoucherType.Trip }, signer) };

        var nights = (proposal.Resources?.Rooms ?? [])
            .GroupBy(r => (HotelId: ProposalValidator.ParseId(r.HotelId), r.Night))
            .Where(g => g.Key.HotelId is not null)
            .OrderBy(g => g.Key.Night);
        foreach (var night in nights)
        {
            vouchers.Add(Signed(new Voucher
            {
                TripRequestId = trip.Id,
                Type = VoucherType.HotelNight,
                HotelId = night.Key.HotelId,
                Night = night.Key.Night,
                Rooms = night.Count()
            }, signer));
        }
        return vouchers;
    }

    private static Voucher Signed(Voucher voucher, VoucherSigner signer)
    {
        voucher.Code = signer.Sign(voucher.Id, voucher.TripRequestId, voucher.Type, voucher.Night);
        return voucher;
    }
}
