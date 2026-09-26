using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripCraft.Application.Identity;
using TripCraft.Application.Resources;

namespace TripCraft.Infrastructure.Resources;

public class GuideConfiguration : IEntityTypeConfiguration<Guide>
{
    public void Configure(EntityTypeBuilder<Guide> builder)
    {
        builder.ToTable("guides", t =>
        {
            t.HasCheckConstraint("ck_guides_max_pax", "max_pax > 0");
            t.HasCheckConstraint("ck_guides_day_rate", "day_rate_lkr > 0");
        });
        builder.Property(g => g.Name).HasMaxLength(100).IsRequired();
        builder.Property(g => g.Phone).HasMaxLength(30).IsRequired();

        // One login per guide profile; the login is optional (a freelance guide may have none).
        builder.HasOne<User>().WithMany().HasForeignKey(g => g.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(g => g.UserId).IsUnique();

        builder.HasMany(g => g.Languages).WithOne().HasForeignKey(l => l.GuideId).OnDelete(DeleteBehavior.Cascade);
    }
}
