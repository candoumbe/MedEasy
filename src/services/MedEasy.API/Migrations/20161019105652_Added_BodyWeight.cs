using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MedEasy.API.Migrations
{
    /// <summary>
    /// Adds<see cref="Objects.BodyWeight"/>
    /// </summary>
    public partial class Added_BodyWeight : Migration
    {
        /// <summary>
        /// Upgrades the database
        /// </summary>
        /// <param name="migrationBuilder"></param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BodyWeight",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatedBy = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(nullable: true),
                    DateOfMeasure = table.Column<DateTimeOffset>(nullable: false),
                    PatientId = table.Column<int>(nullable: false),
                    UpdatedBy = table.Column<string>(nullable: true),
                    UpdatedDate = table.Column<DateTimeOffset>(nullable: true),
                    Value = table.Column<float>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BodyWeight", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BodyWeight_Patient_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "Temperature",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "DateOfMeasure",
                table: "Temperature",
                nullable: false);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedDate",
                table: "Temperature",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "Specialty",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedDate",
                table: "Specialty",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "Patient",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedDate",
                table: "Patient",
                nullable: true);

            migrationBuilder.DropIndex(name : "IX_Patient_BirthDate", table: "Patient");
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "BirthDate",
                table: "Patient",
                nullable: true);
            migrationBuilder.CreateIndex(
               name: "IX_Patient_BirthDate",
               table: "Patient",
               column: "BirthDate");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "Doctor",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedDate",
                table: "Doctor",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedDate",
                table: "BloodPressure",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "DateOfMeasure",
                table: "BloodPressure",
                nullable: false);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedDate",
                table: "BloodPressure",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BodyWeight_PatientId",
                table: "BodyWeight",
                column: "PatientId");
        }
        /// <summary>
        /// Downgrades the database
        /// </summary>
        /// <param name="migrationBuilder"></param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BodyWeight");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                table: "Temperature",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfMeasure",
                table: "Temperature",
                nullable: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Temperature",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                table: "Specialty",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Specialty",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                table: "Patient",
                nullable: true);

            migrationBuilder.DropIndex(name: "IX_Patient_BirthDate", table:"Patient");
            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Patient",
                nullable: true);
            migrationBuilder.CreateIndex(
               name: "IX_Patient_BirthDate",
               table: "Patient",
               column: "BirthDate");

            migrationBuilder.AlterColumn<DateTime>(
                name: "BirthDate",
                table: "Patient",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                table: "Doctor",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Doctor",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                table: "BloodPressure",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfMeasure",
                table: "BloodPressure",
                nullable: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "BloodPressure",
                nullable: true);
        }
    }
}
