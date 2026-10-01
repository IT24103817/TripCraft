namespace TripCraft.Application.Vouchers;

public interface IVoucherRepository
{
    /// <summary>Read-only.</summary>
    IQueryable<Voucher> Query();

    void Add(Voucher voucher);
}
