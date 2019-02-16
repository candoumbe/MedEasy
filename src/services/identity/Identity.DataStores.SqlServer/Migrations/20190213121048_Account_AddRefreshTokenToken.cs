using Microsoft.EntityFrameworkCore.Migrations;

namespace Identity.DataStores.SqlServer.Migrations
{
    public partial class Account_AddRefreshTokenToken : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "Account",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "Account");
        }
    }
}
