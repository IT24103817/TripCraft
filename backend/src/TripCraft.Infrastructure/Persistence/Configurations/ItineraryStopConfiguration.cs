using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripCraft.Application.Trips;

namespace TripCraft.Infrastructure.Persistence.Configurations;

public class ItineraryStopConfiguration : IEntityTypeConfiguration<ItineraryStop>
{
    public void Configure(EntityTypeBuilder<ItineraryStop> builder)
    {
        builder.ToTable("itinerary_stops");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.ArrivalTime).HasColumnType("time");

        // Attractions are soft-deleted, never hard-deleted while an itinerary uses them.
        builder.HasOne(s => s.Attraction)
               .WithMany()
               .HasForeignKey(s => s.AttractionId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new { s.ItineraryDayId, s.Sequence }).IsUnique();
    }
}
