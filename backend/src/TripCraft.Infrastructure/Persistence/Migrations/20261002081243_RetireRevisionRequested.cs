using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TripCraft.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// v1.1: RevisionRequested is no longer a workflow or quotation status. A workflow that was re-planning stays
    /// Planning; any other (an old proposal that waited for a manager's review, its trip now NeedsOperator) becomes
    /// FailedSafely with a summary, as the new code does for a proposal that needs the operator. A replaced
    /// quotation version becomes Superseded. Decision and audit rows are history and are not changed.
    /// </summary>
    public partial class RetireRevisionRequested : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE agent_workflows w SET status = 'Planning'
                WHERE w.status = 'RevisionRequested'
                  AND (SELECT t.status FROM trip_requests t WHERE t.id = w.trip_request_id) = 'Planning';
                """);
            migrationBuilder.Sql("""
                UPDATE agent_workflows
                SET status = 'FailedSafely',
                    current_step = 'failed',
                    error_summary = COALESCE(error_summary, 'The proposal did not pass validation and needs the operator.')
                WHERE status = 'RevisionRequested';
                """);
            migrationBuilder.Sql("UPDATE quotations SET status = 'Superseded' WHERE status = 'RevisionRequested';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Not reversible: the old status of each row is not kept, and the mapped rows are valid in v1.0 too.
        }
    }
}
