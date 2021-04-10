using System;
using Microsoft.EntityFrameworkCore.Migrations;

using NodaTime;

namespace Measures.Context.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Patient",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedDate = table.Column<Instant>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true),
                    UpdatedDate = table.Column<Instant>(nullable: false),
                    UpdatedBy = table.Column<string>(nullable: true),
                    Name = table.Column<string>(maxLength: 255, nullable: true),
                    BirthDate = table.Column<LocalDate>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patient", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BloodPressure",
                columns: table => new
                {
                    PatientId = table.Column<Guid>(nullable: false),
                    DateOfMeasure = table.Column<Instant>(nullable: false),
                    Id = table.Column<Guid>(nullable: false),
                    DiastolicPressure = table.Column<float>(nullable: false),
                    SystolicPressure = table.Column<float>(nullable: false)
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
                    PatientId = table.Column<Guid>(nullable: false),
                    DateOfMeasure = table.Column<Instant>(nullable: false),
                    Id = table.Column<Guid>(nullable: false),
                    Value = table.Column<float>(nullable: false)
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
