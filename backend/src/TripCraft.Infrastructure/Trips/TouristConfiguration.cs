using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripCraft.Application.Identity;
using TripCraft.Application.Trips;

namespace TripCraft.Infrastructure.Trips;

public class TouristConfiguration : IEntityTypeConfiguration<Tourist>
{
    public void Configure(EntityTypeBuilder<Tourist> builder)
    {
        builder.ToTable("tourists");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Nationality).HasMaxLength(100).IsRequired();
        builder.Property(t => t.PassportNumberMasked).HasMaxLength(20).IsRequired();
        builder.Property(t => t.PassportPhotoUrl).HasMaxLength(500);

        // One tourist profile per user. No navigation on User so the Identity module stays untouched.
        builder.HasOne<User>()
               .WithOne()
               .HasForeignKey<Tourist>(t => t.UserId)
               .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(t => t.UserId).IsUnique();
    }
}
