using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TripCraft.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class QuotationsGoStraightToClient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "best_available_price",
                table: "quotations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "over_budget_usd",
                table: "quotations",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: true);

            // v1.1 lifecycle without an operator review before the client: map the removed trip statuses.
            // PendingReview: declined by the client if the newest quotation was declined, otherwise waiting for the
            // operator (it was never sent). RevisionRequested: the agents were re-planning. FailedSafely: the operator
            // decides now (retry, send by hand or cancel).
            migrationBuilder.Sql("""
                UPDATE trip_requests t SET status = 'ClientDeclined'
                WHERE t.status = 'PendingReview'
                  AND (SELECT q.status FROM quotations q WHERE q.trip_request_id = t.id ORDER BY q.version DESC LIMIT 1) = 'Declined';
                """);
            migrationBuilder.Sql("UPDATE trip_requests SET status = 'NeedsOperator' WHERE status IN ('PendingReview', 'FailedSafely');");
            migrationBuilder.Sql("UPDATE trip_requests SET status = 'Planning' WHERE status = 'RevisionRequested';");

            // Revenue now counts Confirm decisions. In v1.0 the manager's Approve also confirmed the trip, so every
            // approved quotation of a trip that went on to be booked gets a matching Confirmed decision (same time).
            migrationBuilder.Sql("""
                INSERT INTO approval_decisions (id, quotation_id, decided_by, decision, comment, decided_at, created_at, updated_at)
                SELECT gen_random_uuid(), d.quotation_id, d.decided_by, 'Confirmed', 'Confirmed by the v1.0 approval', d.decided_at, now(), now()
                FROM approval_decisions d
                JOIN quotations q ON q.id = d.quotation_id
                JOIN trip_requests t ON t.id = q.trip_request_id
                WHERE d.decision = 'Approved' AND t.status IN ('Confirmed', 'InProgress', 'Completed')
                  AND NOT EXISTS (SELECT 1 FROM approval_decisions c WHERE c.quotation_id = d.quotation_id AND c.decision = 'Confirmed');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE trip_requests SET status = 'PendingReview' WHERE status IN ('ClientDeclined', 'NeedsOperator');");

            migrationBuilder.DropColumn(
                name: "best_available_price",
                table: "quotations");

            migrationBuilder.DropColumn(
                name: "over_budget_usd",
                table: "quotations");
        }
    }
}
