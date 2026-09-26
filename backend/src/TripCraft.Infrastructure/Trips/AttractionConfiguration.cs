using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripCraft.Application.Trips;

namespace TripCraft.Infrastructure.Trips;

public class AttractionConfiguration : IEntityTypeConfiguration<Attraction>
{
    public void Configure(EntityTypeBuilder<Attraction> builder)
    {
        builder.ToTable("attractions");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name).HasMaxLength(200).IsRequired();
        builder.Property(a => a.City).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Category).HasMaxLength(50).IsRequired();

        // The Itinerary Analysis agent calls get_attractions(city), so lookups are by city.
        builder.HasIndex(a => a.City);
    }
}
