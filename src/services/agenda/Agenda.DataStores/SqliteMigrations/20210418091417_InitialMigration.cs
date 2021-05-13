using System;

using Microsoft.EntityFrameworkCore.Migrations;

using NodaTime;

namespace Agenda.DataStores.SqliteMigrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Appointment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Location = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Subject = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    StartDate = table.Column<Instant>(type: "timestamp", nullable: false),
                    EndDate = table.Column<Instant>(type: "timestamp", nullable: false),
                    CreatedDate = table.Column<Instant>(type: "timestamp", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    UpdatedDate = table.Column<Instant>(type: "timestamp", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Attendee",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, defaultValue: ""),
                    Email = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<Instant>(type: "timestamp", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    UpdatedDate = table.Column<Instant>(type: "timestamp", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attendee", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppointmentAttendee",
                columns: table => new
                {
                    AppointmentsId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttendeesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentAttendee", x => new { x.AppointmentsId, x.AttendeesId });
                    table.ForeignKey(
                        name: "FK_AppointmentAttendee_Appointment_AppointmentsId",
                        column: x => x.AppointmentsId,
                        principalTable: "Appointment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppointmentAttendee_Attendee_AttendeesId",
                        column: x => x.AttendeesId,
                        principalTable: "Attendee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentAttendee_AttendeesId",
                table: "AppointmentAttendee",
                column: "AttendeesId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppointmentAttendee");

            migrationBuilder.DropTable(
                name: "Appointment");

            migrationBuilder.DropTable(
                name: "Attendee");
        }
    }
}
