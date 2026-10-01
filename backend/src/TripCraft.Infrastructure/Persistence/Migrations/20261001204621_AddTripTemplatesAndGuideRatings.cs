using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TripCraft.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTripTemplatesAndGuideRatings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "guide_ratings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    trip_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    guide_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rated_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    stars = table.Column<int>(type: "integer", nullable: false),
                    comment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_guide_ratings", x => x.id);
                    table.CheckConstraint("ck_guide_ratings_stars", "stars BETWEEN 1 AND 5");
                    table.ForeignKey(
                        name: "fk_guide_ratings_guides_guide_id",
                        column: x => x.guide_id,
                        principalTable: "guides",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_guide_ratings_trip_requests_trip_request_id",
                        column: x => x.trip_request_id,
                        principalTable: "trip_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trip_templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    slug = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    mood_tag = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    summary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    objective = table.Column<string>(type: "text", nullable: false),
                    days = table.Column<int>(type: "integer", nullable: false),
                    cities = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    preferences = table.Column<string>(type: "jsonb", nullable: false),
                    plan = table.Column<string>(type: "jsonb", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trip_templates", x => x.id);
                    table.CheckConstraint("ck_trip_templates_days", "days BETWEEN 1 AND 30");
                });

            migrationBuilder.CreateIndex(
                name: "ix_guide_ratings_guide_id",
                table: "guide_ratings",
                column: "guide_id");

            migrationBuilder.CreateIndex(
                name: "ix_guide_ratings_trip_request_id",
                table: "guide_ratings",
                column: "trip_request_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_trip_templates_slug",
                table: "trip_templates",
                column: "slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "guide_ratings");

            migrationBuilder.DropTable(
                name: "trip_templates");
        }
    }
}
