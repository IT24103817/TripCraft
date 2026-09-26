using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripCraft.Application.Common.Auditing;

namespace TripCraft.Infrastructure.Persistence.Auditing;

/// <summary>audit_logs (PLAN.md section 4). Owned by Component C, who may extend it.</summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Action).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Entity).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Before).HasColumnType("jsonb");
        builder.Property(a => a.After).HasColumnType("jsonb");
    }
}
