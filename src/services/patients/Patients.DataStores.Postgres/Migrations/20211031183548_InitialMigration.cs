using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

namespace Patients.DataStores.Postgres.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Patient",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Firstname = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Lastname = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    BirthDate = table.Column<LocalDate>(type: "date", nullable: true),
                    BirthPlace = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<Instant>(type: "timestamp", nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    UpdatedDate = table.Column<Instant>(type: "timestamp", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patient", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Patient");
        }
    }
}
