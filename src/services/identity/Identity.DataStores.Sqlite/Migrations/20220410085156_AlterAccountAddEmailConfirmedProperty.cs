using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Identity.DataStores.Sqlite.Migrations
{
    public partial class AlterAccountAddEmailConfirmedProperty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EmailConfirmed",
                table: "Account",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailConfirmed",
                table: "Account");
        }
    }
}
