using System;
using Microsoft.EntityFrameworkCore.Migrations;
using static Queries.Core.Builders.Fluent.QueryBuilder;
using Queries.Core;
using Queries.Core.Extensions;
using static Queries.Core.Parts.SpecialValues;
using System.Collections.Generic;

namespace MedEasy.API.Migrations
{
    /// <summary>
    ///  Adds UUID property for all entities
    /// </summary>
    public partial class Added_UUID_Property : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            IEnumerable<string> entities = new[]
            {
                "BodyWeight",
                "BloodPressure",
                "DocumentMetadata",
                "Doctor",
                "Patient",
                "Prescription",
                "PrescriptionItem",
                "Specialty",
                "Temperature"
            };

            foreach (string entity in entities)
            {
                migrationBuilder.AlterColumn<string>(
                   name: "UpdatedBy",
                   table: entity,
                   maxLength: 256,
                   nullable: true);

                migrationBuilder.AlterColumn<string>(
                    name: "CreatedBy",
                    table: entity,
                    maxLength: 256,
                    nullable: true);

                migrationBuilder.AddColumn<Guid>(
                   name: "UUID",
                   table: entity,
                   nullable: true);

                IQuery query = Update(entity).Set("UUID".Field().EqualTo(UUID()));

                migrationBuilder.Sql(query.ForSqlServer());

                migrationBuilder.AlterColumn<Guid>(
                   name: "UUID",
                   table: entity,
                   nullable: false);

                migrationBuilder.CreateIndex(
                    name: $"IX_{entity}_UUID",
                    table:  entity,
                    column: "UUID",
                    unique: true);

                migrationBuilder.AlterColumn<string>(
                    name: "UpdatedBy",
                    table: entity,
                    maxLength: 256,
                    nullable: true);

                migrationBuilder.AlterColumn<string>(
                    name: "CreatedBy",
                    table: entity,
                    maxLength: 256,
                    nullable: true);

            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Temperature_UUID",
                table: "Temperature");

            migrationBuilder.DropIndex(
                name: "IX_Specialty_UUID",
                table: "Specialty");

            migrationBuilder.DropIndex(
                name: "IX_PrescriptionItem_UUID",
                table: "PrescriptionItem");

            migrationBuilder.DropIndex(
                name: "IX_Prescription_UUID",
                table: "Prescription");           

            migrationBuilder.DropIndex(
                name: "IX_Patient_UUID",
                table: "Patient");

            migrationBuilder.DropIndex(
                name: "IX_DocumentMetadata_UUID",
                table: "DocumentMetadata");

            migrationBuilder.DropIndex(
                name: "IX_Doctor_UUID",
                table: "Doctor");

            migrationBuilder.DropIndex(
                name: "IX_BodyWeight_UUID",
                table: "BodyWeight");

            migrationBuilder.DropIndex(
                name: "IX_BloodPressure_UUID",
                table: "BloodPressure");

            migrationBuilder.DropColumn(
                name: "UUID",
                table: "Temperature");

            migrationBuilder.DropColumn(
                name: "UUID",
                table: "Specialty");

            migrationBuilder.DropColumn(
                name: "UUID",
                table: "PrescriptionItem");

            migrationBuilder.DropColumn(
                name: "UUID",
                table: "Prescription");

            migrationBuilder.DropColumn(
                name: "UUID",
                table: "Patient");


            migrationBuilder.DropColumn(
                name: "UUID",
                table: "DocumentMetadata");

            migrationBuilder.DropColumn(
                name: "UUID",
                table: "Doctor");

            migrationBuilder.DropColumn(
                name: "UUID",
                table: "BodyWeight");

            migrationBuilder.DropColumn(
                name: "UUID",
                table: "BloodPressure");

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "Temperature",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Temperature",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "Specialty",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Specialty",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "PrescriptionItem",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "PrescriptionItem",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "Prescription",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Prescription",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "Patient",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Patient",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "DocumentMetadata",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "DocumentMetadata",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "Document",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Document",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "Doctor",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Doctor",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "BodyWeight",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "BodyWeight",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "BloodPressure",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "BloodPressure",
                nullable: true);
        }
    }
}
