using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripCraft.Application.Quotations;
using TripCraft.Application.Trips;
using TripCraft.Application.Workflows;

namespace TripCraft.Infrastructure.Quotations;

public class QuotationConfiguration : IEntityTypeConfiguration<Quotation>
{
    public void Configure(EntityTypeBuilder<Quotation> builder)
    {
        builder.ToTable("quotations", t =>
        {
            t.HasCheckConstraint("ck_quotations_version", "version >= 1");
        builder.Property(q => q.ProposalSnapshot).HasColumnType("jsonb");
            t.HasCheckConstraint("ck_quotations_totals", "subtotal_lkr >= 0 AND total_lkr >= subtotal_lkr AND total_usd >= 0");
            t.HasCheckConstraint("ck_quotations_fx_rate", "fx_rate > 0");
        });
        builder.Property(q => q.Status).HasConversion<string>().HasMaxLength(24).IsRequired();
        builder.Property(q => q.MarginPct).HasPrecision(5, 2);
        builder.Property(q => q.FxRate).HasPrecision(12, 4);

        builder.HasOne<TripRequest>().WithMany().HasForeignKey(q => q.TripRequestId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AgentWorkflow>().WithMany().HasForeignKey(q => q.WorkflowId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(q => q.Lines).WithOne().HasForeignKey(l => l.QuotationId).OnDelete(DeleteBehavior.Cascade);

        // PLAN.md section 4: one row per version of a trip's quotation.
        builder.HasIndex(q => new { q.TripRequestId, q.Version }).IsUnique();
        // The quotation list filters by status and sorts newest first.
        builder.HasIndex(q => new { q.Status, q.CreatedAt });
        builder.Property(q => q.DepositPct).HasColumnType("numeric(5,2)").HasDefaultValue(30m);
    }
}
