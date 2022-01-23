using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

namespace Measures.DataStores.Postgres.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Patient",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    BirthDate = table.Column<LocalDate>(type: "date", nullable: true),
                    CreatedDate = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedDate = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patient", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BloodPressure",
                columns: table => new
                {
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateOfMeasure = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    DiastolicPressure = table.Column<float>(type: "real", nullable: false),
                    SystolicPressure = table.Column<float>(type: "real", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedDate = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BloodPressure", x => new { x.PatientId, x.DateOfMeasure });
                    table.ForeignKey(
                        name: "FK_BloodPressure_Patient_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Temperature",
                columns: table => new
                {
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateOfMeasure = table.Column<Instant>(type: "timestamp with time zone", nullable: false),
                    Value = table.Column<float>(type: "real", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedDate = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedDate = table.Column<Instant>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Temperature", x => new { x.PatientId, x.DateOfMeasure });
                    table.ForeignKey(
                        name: "FK_Temperature_Patient_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BloodPressure_Id",
                table: "BloodPressure",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Temperature_Id",
                table: "Temperature",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BloodPressure");

            migrationBuilder.DropTable(
                name: "Temperature");

            migrationBuilder.DropTable(
                name: "Patient");
        }
    }
}
