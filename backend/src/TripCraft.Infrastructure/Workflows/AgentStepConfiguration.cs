using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripCraft.Application.Workflows;

namespace TripCraft.Infrastructure.Workflows;

/// <summary>agent_steps (PLAN.md section 4). One row per agent step, numbered per workflow.</summary>
public class AgentStepConfiguration : IEntityTypeConfiguration<AgentStep>
{
    public void Configure(EntityTypeBuilder<AgentStep> builder)
    {
        builder.ToTable("agent_steps");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.AgentName).HasMaxLength(32).IsRequired();
        builder.Property(s => s.ToolName).HasMaxLength(200);
        builder.Property(s => s.InputSummary).HasColumnType("jsonb").IsRequired();
        builder.Property(s => s.OutputSummary).HasColumnType("jsonb").IsRequired();
        builder.Property(s => s.ValidationResult).HasColumnType("jsonb").IsRequired();
        builder.Property(s => s.Status).HasMaxLength(32).IsRequired();

        builder.HasOne<AgentWorkflow>()
               .WithMany()
               .HasForeignKey(s => s.WorkflowId)
               .OnDelete(DeleteBehavior.Cascade);

        // Also serves as the index on workflow_id for "steps of this workflow, in order".
        builder.HasIndex(s => new { s.WorkflowId, s.StepNo }).IsUnique();
    }
}
