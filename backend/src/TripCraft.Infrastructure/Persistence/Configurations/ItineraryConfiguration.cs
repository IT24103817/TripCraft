using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripCraft.Application.Trips;

namespace TripCraft.Infrastructure.Persistence.Configurations;

public class ItineraryConfiguration : IEntityTypeConfiguration<Itinerary>
{
    public void Configure(EntityTypeBuilder<Itinerary> builder)
    {
        builder.ToTable("itineraries");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.GeneratedBy).HasConversion<string>().HasMaxLength(16).IsRequired();

        builder.HasOne(i => i.TripRequest)
               .WithMany()
               .HasForeignKey(i => i.TripRequestId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(i => i.TripRequestId).IsUnique();

        builder.HasMany(i => i.Days)
               .WithOne()
               .HasForeignKey(d => d.ItineraryId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
