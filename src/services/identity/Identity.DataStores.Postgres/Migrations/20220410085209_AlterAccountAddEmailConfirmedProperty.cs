using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

#nullable disable

namespace Identity.DataStores.Postgres.Migrations
{
    public partial class AlterAccountAddEmailConfirmedProperty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Instant>(
                name: "UpdatedDate",
                table: "Role",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(Instant),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Instant>(
                name: "CreatedDate",
                table: "Role",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(Instant),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Instant>(
                name: "Start",
                table: "AccountClaim",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "End",
                table: "AccountClaim",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(Instant),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Instant>(
                name: "UpdatedDate",
                table: "Account",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(Instant),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Instant>(
                name: "CreatedDate",
                table: "Account",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(Instant),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailConfirmed",
                table: "Account",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailConfirmed",
                table: "Account");

            migrationBuilder.AlterColumn<Instant>(
                name: "UpdatedDate",
                table: "Role",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Instant>(
                name: "CreatedDate",
                table: "Role",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Instant>(
                name: "Start",
                table: "AccountClaim",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Instant>(
                name: "End",
                table: "AccountClaim",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Instant>(
                name: "UpdatedDate",
                table: "Account",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Instant>(
                name: "CreatedDate",
                table: "Account",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(Instant),
                oldType: "timestamp with time zone",
                oldNullable: true);
        }
    }
}
