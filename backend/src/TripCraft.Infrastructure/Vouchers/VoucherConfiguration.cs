using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripCraft.Application.Resources;
using TripCraft.Application.Trips;
using TripCraft.Application.Vouchers;

namespace TripCraft.Infrastructure.Vouchers;

public class VoucherConfiguration : IEntityTypeConfiguration<Voucher>
{
    public void Configure(EntityTypeBuilder<Voucher> builder)
    {
        builder.ToTable("vouchers", t => t.HasCheckConstraint("ck_vouchers_rooms", "rooms >= 0"));
        builder.Ignore(v => v.QrPayload);
        builder.Property(v => v.Type).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(v => v.Code).HasMaxLength(200).IsRequired();
        builder.HasIndex(v => v.Code).IsUnique();
        builder.HasIndex(v => v.TripRequestId);
        builder.HasOne<TripRequest>().WithMany().HasForeignKey(v => v.TripRequestId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Hotel>().WithMany().HasForeignKey(v => v.HotelId).OnDelete(DeleteBehavior.Restrict);
    }
}
