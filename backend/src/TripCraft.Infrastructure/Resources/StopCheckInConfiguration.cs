using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripCraft.Application.Resources;

namespace TripCraft.Infrastructure.Resources;

public class StopCheckInConfiguration : IEntityTypeConfiguration<StopCheckIn>
{
    public void Configure(EntityTypeBuilder<StopCheckIn> builder)
    {
        builder.ToTable("stop_check_ins", t => t.HasCheckConstraint("ck_stop_check_ins_distance", "distance_meters >= 0"));
        builder.Property(c => c.Method).HasConversion<string>().HasMaxLength(16).IsRequired().HasDefaultValue(CheckInMethod.Gps);
        builder.HasOne<Application.Trips.ItineraryStop>().WithMany().HasForeignKey(c => c.ItineraryStopId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Guide>().WithMany().HasForeignKey(c => c.GuideId).OnDelete(DeleteBehavior.Restrict);
        // One check-in per stop.
        builder.HasIndex(c => c.ItineraryStopId).IsUnique();
    }
}
