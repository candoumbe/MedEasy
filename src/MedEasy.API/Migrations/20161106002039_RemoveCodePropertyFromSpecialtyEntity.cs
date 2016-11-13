using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MedEasy.API.Migrations
{
    public partial class RemoveCodePropertyFromSpecialtyEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "Specialty");

            migrationBuilder.AlterColumn<decimal>(
                name: "Value",
                table: "BodyWeight",
                nullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Specialty",
                maxLength: 5,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<float>(
                name: "Value",
                table: "BodyWeight",
                nullable: false);
        }
    }
}
