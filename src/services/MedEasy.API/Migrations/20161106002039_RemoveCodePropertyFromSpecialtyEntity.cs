using Microsoft.EntityFrameworkCore.Migrations;

namespace MedEasy.API.Migrations
{
    /// <summary>
    /// Removes Code from <see cref="Objects.Specialty"/>
    /// </summary>
    public partial class RemoveCodePropertyFromSpecialtyEntity : Migration
    {
        /// <summary>
        /// upgrades the database
        /// </summary>
        /// <param name="migrationBuilder"></param>
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

        /// <summary>
        /// Downgrades the database
        /// </summary>
        /// <param name="migrationBuilder"></param>
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
