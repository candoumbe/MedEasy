using Microsoft.EntityFrameworkCore.Migrations;

namespace MedEasy.API.Migrations
{
    /// <summary>
    /// Adds Notes field to the Patient repository
    /// </summary>
    public partial class AddedNotesProperty : Migration
    {
        /// <summary>
        /// Upgrades the database
        /// </summary>
        /// <param name="migrationBuilder"></param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Patient",
                nullable: false,
                defaultValue: "");
        }
        /// <summary>
        /// Downgrades the database
        /// </summary>
        /// <param name="migrationBuilder"></param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Patient");
        }
    }
}
