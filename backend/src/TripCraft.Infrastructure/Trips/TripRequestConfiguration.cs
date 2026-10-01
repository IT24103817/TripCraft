using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripCraft.Application.Trips;

namespace TripCraft.Infrastructure.Trips;

public class TripRequestConfiguration : IEntityTypeConfiguration<TripRequest>
{
    public void Configure(EntityTypeBuilder<TripRequest> builder)
    {
        builder.ToTable("trip_requests", t =>
        {
            t.HasCheckConstraint("ck_trip_requests_end_after_start", "end_date >= start_date");
            t.HasCheckConstraint("ck_trip_requests_pax_positive", "pax > 0");
        });
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Objective).HasColumnType("text").IsRequired();
        builder.Property(r => r.StartDate).HasColumnType("date");
        builder.Property(r => r.EndDate).HasColumnType("date");
        builder.Property(r => r.Preferences).HasColumnType("jsonb").IsRequired();
        builder.Ignore(r => r.CityList);
        builder.Property(r => r.Cities).HasMaxLength(500).IsRequired().HasDefaultValue(string.Empty);
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(32).IsRequired();

        builder.HasOne(r => r.Tourist)
               .WithMany()
               .HasForeignKey(r => r.TouristId)
               .OnDelete(DeleteBehavior.Restrict);

        // Supports the list screen: filter by status, sort/filter by start date.
        builder.HasIndex(r => new { r.Status, r.StartDate });
    }
}
