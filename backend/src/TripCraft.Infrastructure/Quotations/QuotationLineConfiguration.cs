using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripCraft.Application.Quotations;

namespace TripCraft.Infrastructure.Quotations;

public class QuotationLineConfiguration : IEntityTypeConfiguration<QuotationLine>
{
    public void Configure(EntityTypeBuilder<QuotationLine> builder)
    {
        builder.ToTable("quotation_lines", t =>
        {
            t.HasCheckConstraint("ck_quotation_lines_type", "line_type IN ('guide', 'vehicle', 'room', 'entry')");
            t.HasCheckConstraint("ck_quotation_lines_amounts", "qty > 0 AND unit_lkr >= 0 AND amount_lkr >= 0");
        });
        builder.Property(l => l.LineType).HasMaxLength(16).IsRequired();
        builder.Property(l => l.Description).HasMaxLength(300).IsRequired();
    }
}
