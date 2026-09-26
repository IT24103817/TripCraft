using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripCraft.Application.Resources;

namespace TripCraft.Infrastructure.Resources;

public class RoomTypeConfiguration : IEntityTypeConfiguration<RoomType>
{
    public void Configure(EntityTypeBuilder<RoomType> builder)
    {
        builder.ToTable("room_types", t =>
        {
            t.HasCheckConstraint("ck_room_types_capacity", "capacity > 0");
            t.HasCheckConstraint("ck_room_types_total_rooms", "total_rooms > 0");
            t.HasCheckConstraint("ck_room_types_rate", "rate_per_night_lkr > 0");
        });
        builder.Property(r => r.Name).HasMaxLength(60).IsRequired();
        builder.HasIndex(r => new { r.HotelId, r.Name }).IsUnique();
    }
}
