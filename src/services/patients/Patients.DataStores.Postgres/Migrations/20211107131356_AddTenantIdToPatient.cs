using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Patients.DataStores.Postgres.Migrations
{
    public partial class AddTenantIdToPatient : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TenantId",
                table: "Patient",
                type: "uuid",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Patient");
        }
    }
}
