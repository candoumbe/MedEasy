using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Identity.DataStores.SqlServer.Migrations
{
    public partial class ReplaceFirstnameLastnameWithName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Account",
                maxLength: 255,
                nullable: true);

            migrationBuilder.DropColumn(
                name: "Firstname",
                table: "Account");

            migrationBuilder.DropColumn(
                name: "Lastname",
                table: "Account");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Account");

            migrationBuilder.AddColumn<string>(
                name: "Firstname",
                table: "Account",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Lastname",
                table: "Account",
                nullable: true);
        }
    }
}
