using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TripCraft.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSettingsAndDeposits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "deposit_paid_at",
                table: "quotations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "deposit_pct",
                table: "quotations",
                type: "numeric(5,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 30m);

            migrationBuilder.CreateTable(
                name: "app_settings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    llm_provider = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    cancellation_cutoff_days = table.Column<int>(type: "integer", nullable: false),
                    deposit_pct = table.Column<decimal>(type: "numeric(5,2)", precision: 12, scale: 2, nullable: false),
                    operator_contact = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_settings", x => x.id);
                    table.CheckConstraint("ck_app_settings_llm_provider", "llm_provider IN ('ollama', 'groq')");
                    table.CheckConstraint("ck_app_settings_ranges", "cancellation_cutoff_days BETWEEN 0 AND 30 AND deposit_pct BETWEEN 0 AND 100");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "app_settings");

            migrationBuilder.DropColumn(
                name: "deposit_paid_at",
                table: "quotations");

            migrationBuilder.DropColumn(
                name: "deposit_pct",
                table: "quotations");
        }
    }
}
