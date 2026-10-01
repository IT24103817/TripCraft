using Microsoft.EntityFrameworkCore;
using TripCraft.Application.Vouchers;
using TripCraft.Infrastructure.Persistence;

namespace TripCraft.Infrastructure.Vouchers;

public class VoucherRepository(AppDbContext db) : IVoucherRepository
{
    public IQueryable<Voucher> Query() => db.Vouchers.AsNoTracking();

    public void Add(Voucher voucher) => db.Vouchers.Add(voucher);
}
