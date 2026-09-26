using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TripCraft.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddResourceManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "guides",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    day_rate_lkr = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    max_pax = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_guides", x => x.id);
                    table.CheckConstraint("ck_guides_day_rate", "day_rate_lkr > 0");
                    table.CheckConstraint("ck_guides_max_pax", "max_pax > 0");
                    table.ForeignKey(
                        name: "fk_guides_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "hotels",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    star_rating = table.Column<int>(type: "integer", nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: false),
                    longitude = table.Column<double>(type: "double precision", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hotels", x => x.id);
                    table.CheckConstraint("ck_hotels_star_rating", "star_rating BETWEEN 1 AND 5");
                });

            migrationBuilder.CreateTable(
                name: "rate_cards",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    margin_pct = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    effective_from = table.Column<DateOnly>(type: "date", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rate_cards", x => x.id);
                    table.CheckConstraint("ck_rate_cards_margin", "margin_pct >= 0 AND margin_pct <= 100");
                });

            migrationBuilder.CreateTable(
                name: "resource_holds",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    resource_type = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    resource_id = table.Column<Guid>(type: "uuid", nullable: false),
                    trip_request_id = table.Column<Guid>(type: "uuid", nullable: true),
                    from_date = table.Column<DateOnly>(type: "date", nullable: false),
                    to_date = table.Column<DateOnly>(type: "date", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    note = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_resource_holds", x => x.id);
                    table.CheckConstraint("ck_resource_holds_dates", "to_date >= from_date");
                    table.CheckConstraint("ck_resource_holds_quantity", "quantity > 0");
                    table.ForeignKey(
                        name: "fk_resource_holds_trip_requests_trip_request_id",
                        column: x => x.trip_request_id,
                        principalTable: "trip_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vehicles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    registration_no = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    seats = table.Column<int>(type: "integer", nullable: false),
                    rate_per_km_lkr = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vehicles", x => x.id);
                    table.CheckConstraint("ck_vehicles_rate", "rate_per_km_lkr > 0");
                    table.CheckConstraint("ck_vehicles_seats", "seats > 0");
                });

            migrationBuilder.CreateTable(
                name: "guide_languages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    guide_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language_code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_guide_languages", x => x.id);
                    table.CheckConstraint("ck_guide_languages_code", "char_length(language_code) = 2");
                    table.ForeignKey(
                        name: "fk_guide_languages_guides_guide_id",
                        column: x => x.guide_id,
                        principalTable: "guides",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "stop_check_ins",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    itinerary_stop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    guide_id = table.Column<Guid>(type: "uuid", nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: false),
                    longitude = table.Column<double>(type: "double precision", nullable: false),
                    distance_meters = table.Column<int>(type: "integer", nullable: false),
                    checked_in_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stop_check_ins", x => x.id);
                    table.CheckConstraint("ck_stop_check_ins_distance", "distance_meters >= 0");
                    table.ForeignKey(
                        name: "fk_stop_check_ins_guides_guide_id",
                        column: x => x.guide_id,
                        principalTable: "guides",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_stop_check_ins_itinerary_stops_itinerary_stop_id",
                        column: x => x.itinerary_stop_id,
                        principalTable: "itinerary_stops",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "room_types",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    hotel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    capacity = table.Column<int>(type: "integer", nullable: false),
                    rate_per_night_lkr = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    total_rooms = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_room_types", x => x.id);
                    table.CheckConstraint("ck_room_types_capacity", "capacity > 0");
                    table.CheckConstraint("ck_room_types_rate", "rate_per_night_lkr > 0");
                    table.CheckConstraint("ck_room_types_total_rooms", "total_rooms > 0");
                    table.ForeignKey(
                        name: "fk_room_types_hotels_hotel_id",
                        column: x => x.hotel_id,
                        principalTable: "hotels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_itinerary_days_hotel_id",
                table: "itinerary_days",
                column: "hotel_id");

            migrationBuilder.CreateIndex(
                name: "ix_guide_languages_guide_id_language_code",
                table: "guide_languages",
                columns: new[] { "guide_id", "language_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_guide_languages_language_code",
                table: "guide_languages",
                column: "language_code");

            migrationBuilder.CreateIndex(
                name: "ix_guides_user_id",
                table: "guides",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_hotels_city",
                table: "hotels",
                column: "city");

            migrationBuilder.CreateIndex(
                name: "ix_rate_cards_effective_from",
                table: "rate_cards",
                column: "effective_from",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_resource_holds_resource_type_resource_id_from_date_to_date",
                table: "resource_holds",
                columns: new[] { "resource_type", "resource_id", "from_date", "to_date" });

            migrationBuilder.CreateIndex(
                name: "ix_resource_holds_trip_request_id",
                table: "resource_holds",
                column: "trip_request_id");

            migrationBuilder.CreateIndex(
                name: "ix_room_types_hotel_id_name",
                table: "room_types",
                columns: new[] { "hotel_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_stop_check_ins_guide_id",
                table: "stop_check_ins",
                column: "guide_id");

            migrationBuilder.CreateIndex(
                name: "ix_stop_check_ins_itinerary_stop_id",
                table: "stop_check_ins",
                column: "itinerary_stop_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vehicles_registration_no",
                table: "vehicles",
                column: "registration_no",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_itinerary_days_hotels_hotel_id",
                table: "itinerary_days",
                column: "hotel_id",
                principalTable: "hotels",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            // No two Held holds of the same guide or vehicle may overlap (dates inclusive). The service checks
            // first for a friendly 409; this constraint also stops two approvals that race each other.
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS btree_gist;");
            migrationBuilder.Sql("""
                ALTER TABLE resource_holds ADD CONSTRAINT ex_resource_holds_no_overlap
                EXCLUDE USING gist (resource_type WITH =, resource_id WITH =, daterange(from_date, to_date, '[]') WITH &&)
                WHERE (status = 'Held' AND resource_type IN ('Guide', 'Vehicle'));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_itinerary_days_hotels_hotel_id",
                table: "itinerary_days");

            migrationBuilder.DropTable(
                name: "guide_languages");

            migrationBuilder.DropTable(
                name: "rate_cards");

            migrationBuilder.DropTable(
                name: "resource_holds");

            migrationBuilder.DropTable(
                name: "room_types");

            migrationBuilder.DropTable(
                name: "stop_check_ins");

            migrationBuilder.DropTable(
                name: "vehicles");

            migrationBuilder.DropTable(
                name: "hotels");

            migrationBuilder.DropTable(
                name: "guides");

            migrationBuilder.DropIndex(
                name: "ix_itinerary_days_hotel_id",
                table: "itinerary_days");
        }
    }
}
