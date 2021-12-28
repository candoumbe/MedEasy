using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Measures.DataStores.Postgres.Migrations
{
    public partial class RenamePatientEntityToSubject : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BloodPressure_Patient_PatientId",
                table: "BloodPressure");

            migrationBuilder.DropForeignKey(
                name: "FK_Temperature_Patient_PatientId",
                table: "Temperature");

            migrationBuilder.DropTable(
                name: "Patient");

            migrationBuilder.RenameColumn(
                name: "PatientId",
                table: "Temperature",
                newName: "SubjectId");

            migrationBuilder.RenameColumn(
                name: "PatientId",
                table: "BloodPressure",
                newName: "SubjectId");

            migrationBuilder.AlterColumn<Instant>(
                name: "UpdatedDate",
                table: "Temperature",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(Instant),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Instant>(
                name: "CreatedDate",
                table: "Temperature",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(Instant),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Instant>(
                name: "DateOfMeasure",
                table: "Temperature",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "UpdatedDate",
                table: "BloodPressure",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(Instant),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Instant>(
                name: "CreatedDate",
                table: "BloodPressure",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(Instant),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Instant>(
                name: "DateOfMeasure",
                table: "BloodPressure",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp without time zone");

            migrationBuilder.CreateTable(
                name: "Subject",
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
                    table.PrimaryKey("PK_Subject", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_BloodPressure_Subject_SubjectId",
                table: "BloodPressure",
                column: "SubjectId",
                principalTable: "Subject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Temperature_Subject_SubjectId",
                table: "Temperature",
                column: "SubjectId",
                principalTable: "Subject",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BloodPressure_Subject_SubjectId",
                table: "BloodPressure");

            migrationBuilder.DropForeignKey(
                name: "FK_Temperature_Subject_SubjectId",
                table: "Temperature");

            migrationBuilder.DropTable(
                name: "Subject");

            migrationBuilder.RenameColumn(
                name: "SubjectId",
                table: "Temperature",
                newName: "PatientId");

            migrationBuilder.RenameColumn(
                name: "SubjectId",
                table: "BloodPressure",
                newName: "PatientId");

            migrationBuilder.AlterColumn<Instant>(
                name: "UpdatedDate",
                table: "Temperature",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Instant>(
                name: "CreatedDate",
                table: "Temperature",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Instant>(
                name: "DateOfMeasure",
                table: "Temperature",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "UpdatedDate",
                table: "BloodPressure",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Instant>(
                name: "CreatedDate",
                table: "BloodPressure",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Instant>(
                name: "DateOfMeasure",
                table: "BloodPressure",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateTable(
                name: "Patient",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BirthDate = table.Column<LocalDate>(type: "date", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<Instant>(type: "timestamp without time zone", nullable: true),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedDate = table.Column<Instant>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patient", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_BloodPressure_Patient_PatientId",
                table: "BloodPressure",
                column: "PatientId",
                principalTable: "Patient",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Temperature_Patient_PatientId",
                table: "Temperature",
                column: "PatientId",
                principalTable: "Patient",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
