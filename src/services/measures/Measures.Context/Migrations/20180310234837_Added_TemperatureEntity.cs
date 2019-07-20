using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Measures.API.Migrations
{
    public partial class Added_TemperatureEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Temperature_Patient_PatientId",
                table: "Temperature");

            migrationBuilder.DropTable(
                name: "BloodPressure");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Temperature",
                table: "Temperature");

            migrationBuilder.RenameTable(
                name: "Temperature",
                newName: "Measures");

            migrationBuilder.RenameIndex(
                name: "IX_Temperature_UUID",
                table: "Measures",
                newName: "IX_Measures_UUID");

            migrationBuilder.RenameIndex(
                name: "IX_Temperature_PatientId",
                table: "Measures",
                newName: "IX_Measures_PatientId");

            migrationBuilder.AlterColumn<float>(
                name: "Value",
                table: "Measures",
                nullable: true,
                oldClrType: typeof(float));

            migrationBuilder.AddColumn<float>(
                name: "DiastolicPressure",
                table: "Measures",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "SystolicPressure",
                table: "Measures",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Measures",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Measures",
                table: "Measures",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Measures_Patient_PatientId",
                table: "Measures",
                column: "PatientId",
                principalTable: "Patient",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Measures_Patient_PatientId",
                table: "Measures");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Measures",
                table: "Measures");

            migrationBuilder.DropColumn(
                name: "DiastolicPressure",
                table: "Measures");

            migrationBuilder.DropColumn(
                name: "SystolicPressure",
                table: "Measures");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Measures");

            migrationBuilder.RenameTable(
                name: "Measures",
                newName: "Temperature");

            migrationBuilder.RenameIndex(
                name: "IX_Measures_UUID",
                table: "Temperature",
                newName: "IX_Temperature_UUID");

            migrationBuilder.RenameIndex(
                name: "IX_Measures_PatientId",
                table: "Temperature",
                newName: "IX_Temperature_PatientId");

            migrationBuilder.AlterColumn<float>(
                name: "Value",
                table: "Temperature",
                nullable: false,
                oldClrType: typeof(float),
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Temperature",
                table: "Temperature",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "BloodPressure",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatedBy = table.Column<string>(maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(nullable: false),
                    DateOfMeasure = table.Column<DateTimeOffset>(nullable: false),
                    DiastolicPressure = table.Column<float>(nullable: false),
                    PatientId = table.Column<int>(nullable: false),
                    SystolicPressure = table.Column<float>(nullable: false),
                    UUID = table.Column<Guid>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 255, nullable: true),
                    UpdatedDate = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BloodPressure", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BloodPressure_Patient_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BloodPressure_PatientId",
                table: "BloodPressure",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_BloodPressure_UUID",
                table: "BloodPressure",
                column: "UUID",
                unique: true);

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
