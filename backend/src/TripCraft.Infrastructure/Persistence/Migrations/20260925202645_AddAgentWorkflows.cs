using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TripCraft.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAgentWorkflows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "validation_result",
                table: "agent_workflows",
                type: "jsonb",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "agent_steps",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workflow_id = table.Column<Guid>(type: "uuid", nullable: false),
                    step_no = table.Column<int>(type: "integer", nullable: false),
                    agent_name = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    tool_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    input_summary = table.Column<string>(type: "jsonb", nullable: false),
                    output_summary = table.Column<string>(type: "jsonb", nullable: false),
                    validation_result = table.Column<string>(type: "jsonb", nullable: false),
                    duration_ms = table.Column<int>(type: "integer", nullable: false),
                    retries = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agent_steps", x => x.id);
                    table.ForeignKey(
                        name: "fk_agent_steps_agent_workflows_workflow_id",
                        column: x => x.workflow_id,
                        principalTable: "agent_workflows",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "city_distances",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    from_city = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    to_city = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    distance_km = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    duration_minutes = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_city_distances", x => x.id);
                    table.CheckConstraint("ck_city_distances_positive", "distance_km > 0 AND duration_minutes > 0");
                });

            migrationBuilder.CreateIndex(
                name: "ix_agent_workflows_status_started_at",
                table: "agent_workflows",
                columns: new[] { "status", "started_at" });

            migrationBuilder.CreateIndex(
                name: "ix_agent_steps_workflow_id_step_no",
                table: "agent_steps",
                columns: new[] { "workflow_id", "step_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_city_distances_from_city_to_city",
                table: "city_distances",
                columns: new[] { "from_city", "to_city" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agent_steps");

            migrationBuilder.DropTable(
                name: "city_distances");

            migrationBuilder.DropIndex(
                name: "ix_agent_workflows_status_started_at",
                table: "agent_workflows");

            migrationBuilder.DropColumn(
                name: "validation_result",
                table: "agent_workflows");
        }
    }
}
