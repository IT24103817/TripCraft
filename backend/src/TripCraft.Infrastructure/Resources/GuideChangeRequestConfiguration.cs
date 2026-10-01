using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripCraft.Application.Resources;
using TripCraft.Application.Trips;

namespace TripCraft.Infrastructure.Resources;

public class GuideChangeRequestConfiguration : IEntityTypeConfiguration<GuideChangeRequest>
{
    public void Configure(EntityTypeBuilder<GuideChangeRequest> builder)
    {
        builder.ToTable("guide_change_requests");
        builder.Property(r => r.Reason).HasMaxLength(1000).IsRequired();
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.HasOne<TripRequest>().WithMany().HasForeignKey(r => r.TripRequestId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Guide>().WithMany().HasForeignKey(r => r.GuideId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Guide>().WithMany().HasForeignKey(r => r.ReplacementGuideId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(r => new { r.Status, r.CreatedAt });
    }
}
