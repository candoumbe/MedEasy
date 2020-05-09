using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Documents.DataStore.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Document",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 255, nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: false),
                    UpdatedBy = table.Column<string>(maxLength: 255, nullable: true),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    MimeType = table.Column<string>(maxLength: 255, nullable: false, defaultValue: "application/octect-stream"),
                    Hash = table.Column<string>(nullable: true),
                    Size = table.Column<long>(nullable: false),
                    Status = table.Column<string>(nullable: false, defaultValue: "Ongoing")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Document", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentPart",
                columns: table => new
                {
                    Position = table.Column<int>(nullable: false),
                    DocumentId = table.Column<Guid>(nullable: false),
                    Content = table.Column<byte[]>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentPart", x => new { x.DocumentId, x.Position });
                    table.ForeignKey(
                        name: "FK_DocumentPart_Document_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Document",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentPart");

            migrationBuilder.DropTable(
                name: "Document");
        }
    }
}
