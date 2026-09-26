using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripCraft.Application.Resources;

namespace TripCraft.Infrastructure.Resources;

public class RateCardEntryConfiguration : IEntityTypeConfiguration<RateCardEntry>
{
    public void Configure(EntityTypeBuilder<RateCardEntry> builder)
    {
        builder.ToTable("rate_cards", t => t.HasCheckConstraint("ck_rate_cards_margin", "margin_pct >= 0 AND margin_pct <= 100"));
        builder.Property(r => r.MarginPct).HasPrecision(5, 2);
        builder.HasIndex(r => r.EffectiveFrom).IsUnique();
    }
}
