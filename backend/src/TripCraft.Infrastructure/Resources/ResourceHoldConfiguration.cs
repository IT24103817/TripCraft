using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripCraft.Application.Resources;

namespace TripCraft.Infrastructure.Resources;

public class ResourceHoldConfiguration : IEntityTypeConfiguration<ResourceHold>
{
    public void Configure(EntityTypeBuilder<ResourceHold> builder)
    {
        builder.ToTable("resource_holds", t =>
        {
            t.HasCheckConstraint("ck_resource_holds_dates", "to_date >= from_date");
            t.HasCheckConstraint("ck_resource_holds_quantity", "quantity > 0");
        });
        builder.Property(h => h.ResourceType).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(h => h.Status).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(h => h.Note).HasMaxLength(300);
        builder.HasOne<Application.Trips.TripRequest>().WithMany().HasForeignKey(h => h.TripRequestId)
            .OnDelete(DeleteBehavior.Restrict);
        // PLAN.md section 4: the overlap check is a range query on exactly these columns.
        builder.HasIndex(h => new { h.ResourceType, h.ResourceId, h.FromDate, h.ToDate });
        // The exclusion constraint (no overlapping Held guide/vehicle holds) is added in the migration with SQL.
    }
}
