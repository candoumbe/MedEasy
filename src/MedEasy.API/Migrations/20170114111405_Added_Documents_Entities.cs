using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MedEasy.API.Migrations
{
    public partial class Added_Documents_Entities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentMetadata",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatedBy = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(nullable: true),
                    DocumentId = table.Column<int>(nullable: false),
                    MimeType = table.Column<string>(maxLength: 256, nullable: false),
                    PatientId = table.Column<int>(nullable: false),
                    Size = table.Column<long>(nullable: false),
                    Title = table.Column<string>(maxLength: 256, nullable: false),
                    UpdatedBy = table.Column<string>(nullable: true),
                    UpdatedDate = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentMetadata", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentMetadata_Patient_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Document",
                columns: table => new
                {
                    DocumentMetadataId = table.Column<int>(nullable: false),
                    Content = table.Column<byte[]>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(nullable: true),
                    UpdatedBy = table.Column<string>(nullable: true),
                    UpdatedDate = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Document", x => x.DocumentMetadataId);
                    table.ForeignKey(
                        name: "FK_Document_DocumentMetadata_DocumentMetadataId",
                        column: x => x.DocumentMetadataId,
                        principalTable: "DocumentMetadata",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentMetadata_MimeType",
                table: "DocumentMetadata",
                column: "MimeType");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentMetadata_PatientId",
                table: "DocumentMetadata",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentMetadata_Title",
                table: "DocumentMetadata",
                column: "Title");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Document");

            migrationBuilder.DropTable(
                name: "DocumentMetadata");
        }
    }
}
