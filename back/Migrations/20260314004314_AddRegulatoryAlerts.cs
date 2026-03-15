using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace back.Migrations
{
    /// <inheritdoc />
    public partial class AddRegulatoryAlerts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LegalReferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ArticleOrSection = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RawText = table.Column<string>(type: "text", nullable: false),
                    Jurisdiction = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExtractedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegalReferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LegalReferences_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegulatoryUpdates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    LawIdentifier = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    SourceUrl = table.Column<string>(type: "text", nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsProcessed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegulatoryUpdates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegulatoryAlerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegulatoryUpdateId = table.Column<Guid>(type: "uuid", nullable: false),
                    LegalReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    IsDismissed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegulatoryAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegulatoryAlerts_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegulatoryAlerts_LegalReferences_LegalReferenceId",
                        column: x => x.LegalReferenceId,
                        principalTable: "LegalReferences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegulatoryAlerts_RegulatoryUpdates_RegulatoryUpdateId",
                        column: x => x.RegulatoryUpdateId,
                        principalTable: "RegulatoryUpdates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegulatoryAlerts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LegalReferences_DocumentId",
                table: "LegalReferences",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_RegulatoryAlerts_DocumentId",
                table: "RegulatoryAlerts",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_RegulatoryAlerts_IsRead",
                table: "RegulatoryAlerts",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_RegulatoryAlerts_LegalReferenceId",
                table: "RegulatoryAlerts",
                column: "LegalReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_RegulatoryAlerts_RegulatoryUpdateId",
                table: "RegulatoryAlerts",
                column: "RegulatoryUpdateId");

            migrationBuilder.CreateIndex(
                name: "IX_RegulatoryAlerts_UserId",
                table: "RegulatoryAlerts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RegulatoryUpdates_IsProcessed",
                table: "RegulatoryUpdates",
                column: "IsProcessed");

            migrationBuilder.CreateIndex(
                name: "IX_RegulatoryUpdates_PublishedAt",
                table: "RegulatoryUpdates",
                column: "PublishedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegulatoryAlerts");

            migrationBuilder.DropTable(
                name: "LegalReferences");

            migrationBuilder.DropTable(
                name: "RegulatoryUpdates");
        }
    }
}
