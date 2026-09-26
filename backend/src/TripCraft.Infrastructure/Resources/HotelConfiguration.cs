using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripCraft.Application.Resources;

namespace TripCraft.Infrastructure.Resources;

public class HotelConfiguration : IEntityTypeConfiguration<Hotel>
{
    public void Configure(EntityTypeBuilder<Hotel> builder)
    {
        builder.ToTable("hotels", t => t.HasCheckConstraint("ck_hotels_star_rating", "star_rating BETWEEN 1 AND 5"));
        builder.Property(h => h.Name).HasMaxLength(150).IsRequired();
        builder.Property(h => h.City).HasMaxLength(100).IsRequired();
        // Room availability is searched by city.
        builder.HasIndex(h => h.City);
        builder.HasMany(h => h.RoomTypes).WithOne(r => r.Hotel).HasForeignKey(r => r.HotelId).OnDelete(DeleteBehavior.Restrict);

        // itinerary_days.hotel_id (Component A's table) now points at a real hotel.
        builder.HasMany<Application.Trips.ItineraryDay>().WithOne().HasForeignKey(d => d.HotelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
