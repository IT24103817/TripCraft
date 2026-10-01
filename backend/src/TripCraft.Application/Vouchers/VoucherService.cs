using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Common.Exceptions;
using TripCraft.Application.Common.Security;
using TripCraft.Application.Identity;
using TripCraft.Application.Resources;
using TripCraft.Application.Trips;

namespace TripCraft.Application.Vouchers;

public interface IVoucherService
{
    Task<IReadOnlyList<VoucherDto>> ListAsync(CurrentUser user, Guid tripRequestId, CancellationToken ct);
    Task<byte[]> RenderPdfAsync(CurrentUser user, Guid tripRequestId, CancellationToken ct);
    Task<VoucherVerificationDto> VerifyAsync(string code, CancellationToken ct);
}

/// <summary>The vouchers issued by Confirm: the tourist sees their own, a manager any trip's.</summary>
public class VoucherService(
    IVoucherRepository vouchers,
    ITripRequestRepository trips,
    IResourceRepository resources,
    IUserRepository users,
    VoucherSigner signer,
    IVoucherPdfRenderer pdf) : IVoucherService
{
    public async Task<IReadOnlyList<VoucherDto>> ListAsync(CurrentUser user, Guid tripRequestId, CancellationToken ct) =>
        (await LoadAsync(user, tripRequestId, ct)).Vouchers;

    public async Task<byte[]> RenderPdfAsync(CurrentUser user, Guid tripRequestId, CancellationToken ct)
    {
        var sheet = await LoadAsync(user, tripRequestId, ct);
        if (sheet.Vouchers.Count == 0)
            throw new ConflictException("This trip has no vouchers yet; they are issued when the trip is confirmed.");
        return pdf.Render(sheet);
    }

    /// <summary>Signature check plus "was it issued". Never says more than needed about a forged code.</summary>
    public async Task<VoucherVerificationDto> VerifyAsync(string code, CancellationToken ct)
    {
        var claims = signer.Verify(code);
        if (claims is null)
            return new VoucherVerificationDto(false, "Not a valid TripCraft voucher.", null, null, null);
        if (!await vouchers.Query().AnyAsync(v => v.Id == claims.VoucherId && v.TripRequestId == claims.TripRequestId, ct))
            return new VoucherVerificationDto(false, "This voucher was not issued by TripCraft.", null, null, null);
        return new VoucherVerificationDto(true, "Valid voucher.", claims.TripRequestId, claims.Type.ToString(), claims.Night);
    }

    private async Task<TripVouchersDto> LoadAsync(CurrentUser user, Guid tripRequestId, CancellationToken ct)
    {
        var trip = await trips.GetByIdAsync(tripRequestId, ct) ?? throw new NotFoundException("Trip request not found.");
        TripAccess.EnsureCanAccess(user, trip.Tourist?.UserId);

        // Trip voucher first, then the hotel nights in date order (sorted here: the type is stored as text).
        var rows = (await vouchers.Query().Where(v => v.TripRequestId == trip.Id).ToListAsync(ct))
            .OrderBy(v => v.Type == VoucherType.Trip ? 0 : 1).ThenBy(v => v.Night).ToList();
        var hotelIds = rows.Where(v => v.HotelId != null).Select(v => v.HotelId!.Value).Distinct().ToList();
        var hotels = await resources.Hotels().Where(h => hotelIds.Contains(h.Id)).ToDictionaryAsync(h => h.Id, h => h.Name, ct);
        var tourist = trip.Tourist is null ? null : await users.GetByIdAsync(trip.Tourist.UserId, ct);

        var items = rows.Select(v => new VoucherDto(v.Id, v.Type.ToString(), v.HotelId,
            v.HotelId is { } id ? hotels.GetValueOrDefault(id) : null, v.Night, v.Rooms, v.Code, v.QrPayload)).ToList();
        return new TripVouchersDto(trip.Id, trip.Objective, trip.StartDate, trip.EndDate, trip.Pax,
            tourist?.FullName ?? "Guest", trip.CityList, items);
    }
}
