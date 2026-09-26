using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripCraft.Application.Resources;

namespace TripCraft.Infrastructure.Resources;

public class GuideLanguageConfiguration : IEntityTypeConfiguration<GuideLanguage>
{
    public void Configure(EntityTypeBuilder<GuideLanguage> builder)
    {
        builder.ToTable("guide_languages", t => t.HasCheckConstraint("ck_guide_languages_code", "char_length(language_code) = 2"));
        builder.Property(l => l.LanguageCode).HasMaxLength(2).IsRequired();
        builder.HasIndex(l => new { l.GuideId, l.LanguageCode }).IsUnique();
        // Availability search filters guides by language.
        builder.HasIndex(l => l.LanguageCode);
    }
}
