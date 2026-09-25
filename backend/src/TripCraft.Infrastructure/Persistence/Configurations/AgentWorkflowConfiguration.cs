using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TripCraft.Application.Trips;
using TripCraft.Application.Workflows;

namespace TripCraft.Infrastructure.Persistence.Configurations;

/// <summary>agent_workflows (PLAN.md section 4). Owned by Component C, who may extend it.</summary>
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
        builder.Property(w => w.ErrorSummary).HasColumnType("text");

        builder.HasOne<TripRequest>()
               .WithMany()
               .HasForeignKey(w => w.TripRequestId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
