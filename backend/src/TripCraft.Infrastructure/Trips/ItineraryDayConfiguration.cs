using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripCraft.Application.Trips;

namespace TripCraft.Infrastructure.Trips;

public class ItineraryDayConfiguration : IEntityTypeConfiguration<ItineraryDay>
{
    public void Configure(EntityTypeBuilder<ItineraryDay> builder)
    {
        builder.ToTable("itinerary_days");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.City).HasMaxLength(100).IsRequired();
        builder.Property(d => d.Notes).HasColumnType("text");

        // TODO(Component B): configure HasOne<Hotel>().WithMany().HasForeignKey(d => d.HotelId)
        // when the Hotel entity is added.

        builder.HasIndex(d => new { d.ItineraryId, d.DayNumber }).IsUnique();

        builder.HasMany(d => d.Stops)
               .WithOne()
               .HasForeignKey(s => s.ItineraryDayId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
