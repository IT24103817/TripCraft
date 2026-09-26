using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripCraft.Application.Identity;
using TripCraft.Application.Quotations;

namespace TripCraft.Infrastructure.Quotations;

public class ApprovalDecisionConfiguration : IEntityTypeConfiguration<ApprovalDecision>
{
    public void Configure(EntityTypeBuilder<ApprovalDecision> builder)
    {
        builder.ToTable("approval_decisions");
        builder.Property(d => d.Decision).HasConversion<string>().HasMaxLength(24).IsRequired();
        builder.Property(d => d.Comment).HasMaxLength(1000);
        builder.HasOne<Quotation>().WithMany().HasForeignKey(d => d.QuotationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(d => d.DecidedBy).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(d => d.QuotationId);
    }
}
