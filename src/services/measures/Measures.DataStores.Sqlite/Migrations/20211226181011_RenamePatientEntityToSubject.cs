using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Queries.Core.Builders;
using static Queries.Core.Builders.Fluent.QueryBuilder;

#nullable disable

namespace Measures.DataStores.Sqlite.Migrations
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

            migrationBuilder.CreateTable(
                name: "Subject",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    BirthDate = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedDate = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedDate = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true)
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

            migrationBuilder.RenameColumn(
                name: "PatientId",
                table: "Temperature",
                newName: "SubjectId");

            migrationBuilder.RenameColumn(
                name: "PatientId",
                table: "BloodPressure",
                newName: "SubjectId");

            InsertIntoQuery insertInto = InsertInto("Subject")
                .Values(Select("Id", "Name", "BirthDate", "CreatedDate", "CreatedBy", "UpdatedDate", "UpdatedBy")
                            .From("Patient")
                            .Build())
                .Build();

            migrationBuilder.Sql(insertInto.ForSqlite());

            migrationBuilder.DropTable(
                name: "Patient");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BloodPressure_Subject_SubjectId",
                table: "BloodPressure");

            migrationBuilder.DropForeignKey(
                name: "FK_Temperature_Subject_SubjectId",
                table: "Temperature");

            migrationBuilder.RenameColumn(
                name: "SubjectId",
                table: "Temperature",
                newName: "PatientId");

            migrationBuilder.RenameColumn(
                name: "SubjectId",
                table: "BloodPressure",
                newName: "PatientId");



            migrationBuilder.CreateTable(
                name: "Patient",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BirthDate = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedDate = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedDate = table.Column<string>(type: "TEXT", nullable: true)
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

            InsertIntoQuery insertInto = InsertInto("Patient")
                .Values(Select("Id", "Name", "BirthDate", "CreatedDate", "CreatedBy", "UpdatedDate", "UpdatedBy")
                            .From("Subject")
                            .Build())
                .Build();

            migrationBuilder.Sql(insertInto.ForSqlite());

            migrationBuilder.DropTable(
                name: "Subject");
        }
    }
}
