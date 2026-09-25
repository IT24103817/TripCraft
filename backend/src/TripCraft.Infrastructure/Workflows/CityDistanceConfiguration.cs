using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripCraft.Application.Workflows.External;

namespace TripCraft.Infrastructure.Workflows;

/// <summary>city_distances: static fallback for OpenRouteService.</summary>
public class CityDistanceConfiguration : IEntityTypeConfiguration<CityDistance>
{
    public void Configure(EntityTypeBuilder<CityDistance> builder)
    {
        builder.ToTable("city_distances", t =>
        {
            t.HasCheckConstraint("ck_city_distances_positive", "distance_km > 0 AND duration_minutes > 0");
        });
        builder.HasKey(d => d.Id);
        builder.Property(d => d.FromCity).HasMaxLength(60).IsRequired();
        builder.Property(d => d.ToCity).HasMaxLength(60).IsRequired();
        builder.HasIndex(d => new { d.FromCity, d.ToCity }).IsUnique();
    }
}
