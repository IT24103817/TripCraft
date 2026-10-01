using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripCraft.Application.Trips;

namespace TripCraft.Infrastructure.Trips;

public class TripTemplateConfiguration : IEntityTypeConfiguration<TripTemplate>
{
    public void Configure(EntityTypeBuilder<TripTemplate> builder)
    {
        builder.ToTable("trip_templates", t => t.HasCheckConstraint("ck_trip_templates_days", "days BETWEEN 1 AND 30"));
        builder.Property(t => t.Slug).HasMaxLength(60).IsRequired();
        builder.HasIndex(t => t.Slug).IsUnique();
        builder.Property(t => t.Name).HasMaxLength(100).IsRequired();
        builder.Property(t => t.MoodTag).HasMaxLength(40).IsRequired();
        builder.Property(t => t.Summary).HasMaxLength(500).IsRequired();
        builder.Property(t => t.Objective).HasColumnType("text").IsRequired();
        builder.Property(t => t.Cities).HasMaxLength(500).IsRequired();
        builder.Ignore(t => t.CityList);
        builder.Property(t => t.Preferences).HasColumnType("jsonb").IsRequired();
        builder.Property(t => t.Plan).HasColumnType("jsonb").IsRequired();
    }
}
