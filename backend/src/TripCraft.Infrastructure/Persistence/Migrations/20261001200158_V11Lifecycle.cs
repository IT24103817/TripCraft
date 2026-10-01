using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TripCraft.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class V11Lifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "must_change_password",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "cities",
                table: "trip_requests",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<double>(
                name: "longitude",
                table: "stop_check_ins",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<double>(
                name: "latitude",
                table: "stop_check_ins",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<int>(
                name: "distance_meters",
                table: "stop_check_ins",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "method",
                table: "stop_check_ins",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "Gps");

            migrationBuilder.AddColumn<Guid>(
                name: "voucher_id",
                table: "stop_check_ins",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "proposal_snapshot",
                table: "quotations",
                type: "jsonb",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "email_outbox",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    to = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    subject = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    body = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    trip_request_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_email_outbox", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "guide_change_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    trip_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    guide_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    replacement_guide_id = table.Column<Guid>(type: "uuid", nullable: true),
                    resolved_by = table.Column<Guid>(type: "uuid", nullable: true),
                    resolved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_guide_change_requests", x => x.id);
                    table.ForeignKey(
                        name: "fk_guide_change_requests_guides_guide_id",
                        column: x => x.guide_id,
                        principalTable: "guides",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_guide_change_requests_guides_replacement_guide_id",
                        column: x => x.replacement_guide_id,
                        principalTable: "guides",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_guide_change_requests_trip_requests_trip_request_id",
                        column: x => x.trip_request_id,
                        principalTable: "trip_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    body = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    trip_request_id = table.Column<Guid>(type: "uuid", nullable: true),
                    read_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications", x => x.id);
                    table.ForeignKey(
                        name: "fk_notifications_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vouchers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    trip_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    hotel_id = table.Column<Guid>(type: "uuid", nullable: true),
                    night = table.Column<DateOnly>(type: "date", nullable: true),
                    rooms = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vouchers", x => x.id);
                    table.CheckConstraint("ck_vouchers_rooms", "rooms >= 0");
                    table.ForeignKey(
                        name: "fk_vouchers_hotels_hotel_id",
                        column: x => x.hotel_id,
                        principalTable: "hotels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_vouchers_trip_requests_trip_request_id",
                        column: x => x.trip_request_id,
                        principalTable: "trip_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_email_outbox_sent_at",
                table: "email_outbox",
                column: "sent_at");

            migrationBuilder.CreateIndex(
                name: "ix_guide_change_requests_guide_id",
                table: "guide_change_requests",
                column: "guide_id");

            migrationBuilder.CreateIndex(
                name: "ix_guide_change_requests_replacement_guide_id",
                table: "guide_change_requests",
                column: "replacement_guide_id");

            migrationBuilder.CreateIndex(
                name: "ix_guide_change_requests_status_created_at",
                table: "guide_change_requests",
                columns: new[] { "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_guide_change_requests_trip_request_id",
                table: "guide_change_requests",
                column: "trip_request_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_id_created_at",
                table: "notifications",
                columns: new[] { "user_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_vouchers_code",
                table: "vouchers",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vouchers_hotel_id",
                table: "vouchers",
                column: "hotel_id");

            migrationBuilder.CreateIndex(
                name: "ix_vouchers_trip_request_id",
                table: "vouchers",
                column: "trip_request_id");

            // v1.1 trip lifecycle: statuses are stored as text, so existing rows are renamed here.
            // PendingApproval → PendingReview; Approved (never set by v1.0) → QuotationSent; Rejected → Cancelled.
            migrationBuilder.Sql("UPDATE trip_requests SET status = 'PendingReview' WHERE status = 'PendingApproval';");
            migrationBuilder.Sql("UPDATE trip_requests SET status = 'QuotationSent' WHERE status = 'Approved';");
            migrationBuilder.Sql("UPDATE trip_requests SET status = 'Cancelled' WHERE status = 'Rejected';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE trip_requests SET status = 'PendingApproval' WHERE status = 'PendingReview';");
            migrationBuilder.Sql("UPDATE trip_requests SET status = 'Submitted' WHERE status = 'FailedSafely';");
            migrationBuilder.Sql("UPDATE trip_requests SET status = 'Confirmed' WHERE status IN ('QuotationSent', 'ClientAccepted');");

            migrationBuilder.DropTable(
                name: "email_outbox");

            migrationBuilder.DropTable(
                name: "guide_change_requests");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "vouchers");

            migrationBuilder.DropColumn(
                name: "must_change_password",
                table: "users");

            migrationBuilder.DropColumn(
                name: "cities",
                table: "trip_requests");

            migrationBuilder.DropColumn(
                name: "method",
                table: "stop_check_ins");

            migrationBuilder.DropColumn(
                name: "voucher_id",
                table: "stop_check_ins");

            migrationBuilder.DropColumn(
                name: "proposal_snapshot",
                table: "quotations");

            migrationBuilder.AlterColumn<double>(
                name: "longitude",
                table: "stop_check_ins",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "latitude",
                table: "stop_check_ins",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "distance_meters",
                table: "stop_check_ins",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
