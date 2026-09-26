using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripCraft.Application.Trips;
using TripCraft.Application.Workflows;

namespace TripCraft.Infrastructure.Workflows;

/// <summary>agent_workflows (PLAN.md section 4). Status is stored as text.</summary>
public class AgentWorkflowConfiguration : IEntityTypeConfiguration<AgentWorkflow>
{
    public void Configure(EntityTypeBuilder<AgentWorkflow> builder)
    {
        builder.ToTable("agent_workflows");
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Objective).HasColumnType("text").IsRequired();
        builder.Property(w => w.Plan).HasColumnType("jsonb").IsRequired();
        builder.Property(w => w.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(w => w.CurrentStep).HasMaxLength(100);
        builder.Property(w => w.FinalOutcome).HasColumnType("jsonb");
        builder.Property(w => w.ValidationResult).HasColumnType("jsonb");
        builder.Property(w => w.ErrorSummary).HasColumnType("text");
        builder.Ignore(w => w.IsActive);

        // The approval inbox and workflow monitor filter by status and sort by start time.
        builder.HasIndex(w => w.TripRequestId);
        builder.HasIndex(w => new { w.Status, w.StartedAt });

        builder.HasOne<TripRequest>()
               .WithMany()
               .HasForeignKey(w => w.TripRequestId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
