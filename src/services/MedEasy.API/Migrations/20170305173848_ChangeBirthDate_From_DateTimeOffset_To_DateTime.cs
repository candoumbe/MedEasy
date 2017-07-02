using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MedEasy.API.Migrations
{
    /// <summary>
    /// Changes the <see cref="Objects.Patient.BirthDate"/> type and creates <see cref="Objects.Disease"/> table 
    /// </summary>
    /// <remarks>
    /// This migration also add <see cref="Objects.Document.UUID"/> column.
    /// </remarks>
    public partial class ChangeBirthDate_From_DateTimeOffset_To_DateTime : Migration
    {
        /// <summary>
        /// Upgrades the database
        /// </summary>
        /// <param name="migrationBuilder"></param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name : "IX_Patient_BirthDate", table: "Patient");

            migrationBuilder.AlterColumn<DateTime>(
                name: "BirthDate",
                table: "Patient",
                nullable: true);

            migrationBuilder.CreateIndex(name: "IX_Patient_BirthDate", table : "Patient", column: "BirthDate");

            migrationBuilder.AddColumn<Guid>(
                name: "UUID",
                table: "Document",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Disease",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(maxLength: 20, nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 256, nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(nullable: true),
                    UUID = table.Column<Guid>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 256, nullable: true),
                    UpdatedDate = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Disease", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Disease_Code",
                table: "Disease",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Disease_UUID",
                table: "Disease",
                column: "UUID",
                unique: true);
        }
        /// <summary>
        /// Downgrades the database
        /// </summary>
        /// <param name="migrationBuilder"></param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Disease");

            migrationBuilder.DropColumn(
                name: "UUID",
                table: "Document");

            migrationBuilder.DropIndex(name: "IX_Patient_BirthDate", table: "Patient");


            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "BirthDate",
                table: "Patient",
                nullable: true);

            migrationBuilder.CreateIndex("IX_Patient_BirthDate", "Patient", "BirthDate");

        }
    }
}
