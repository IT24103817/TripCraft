using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripCraft.Application.Resources;
using TripCraft.Application.Trips;

namespace TripCraft.Infrastructure.Resources;

public class GuideRatingConfiguration : IEntityTypeConfiguration<GuideRating>
{
    public void Configure(EntityTypeBuilder<GuideRating> builder)
    {
        builder.ToTable("guide_ratings", t => t.HasCheckConstraint("ck_guide_ratings_stars", "stars BETWEEN 1 AND 5"));
        builder.Property(r => r.Comment).HasMaxLength(500);
        builder.HasIndex(r => r.TripRequestId).IsUnique(); // one rating per trip
        builder.HasIndex(r => r.GuideId);
        builder.HasOne<TripRequest>().WithMany().HasForeignKey(r => r.TripRequestId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Guide>().WithMany().HasForeignKey(r => r.GuideId).OnDelete(DeleteBehavior.Restrict);
    }
}
