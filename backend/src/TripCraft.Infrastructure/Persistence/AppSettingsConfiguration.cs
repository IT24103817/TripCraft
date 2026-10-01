using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripCraft.Application.Common.Settings;

namespace TripCraft.Infrastructure.Persistence;

public class AppSettingsConfiguration : IEntityTypeConfiguration<AppSettings>
{
    public void Configure(EntityTypeBuilder<AppSettings> builder)
    {
        builder.ToTable("app_settings", t =>
        {
            t.HasCheckConstraint("ck_app_settings_llm_provider", "llm_provider IN ('ollama', 'groq')");
            t.HasCheckConstraint("ck_app_settings_ranges",
                "cancellation_cutoff_days BETWEEN 0 AND 30 AND deposit_pct BETWEEN 0 AND 100");
        });
        builder.Property(s => s.LlmProvider).HasMaxLength(16).IsRequired();
        builder.Property(s => s.DepositPct).HasColumnType("numeric(5,2)");
        builder.Property(s => s.OperatorContact).HasMaxLength(200).IsRequired();
    }
}
