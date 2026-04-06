using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace back.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentTopicsAndRegulatoryFileSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegulatoryAlerts_LegalReferences_LegalReferenceId",
                table: "RegulatoryAlerts");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "RegulatoryUpdates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StoredFileName",
                table: "RegulatoryUpdates",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "LegalReferenceId",
                table: "RegulatoryAlerts",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "RiskDescription",
                table: "RegulatoryAlerts",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DocumentTopics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Topic = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ExtractedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTopics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentTopics_Documents_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTopics_DocumentId",
                table: "DocumentTopics",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTopics_Topic",
                table: "DocumentTopics",
                column: "Topic");

            migrationBuilder.AddForeignKey(
                name: "FK_RegulatoryAlerts_LegalReferences_LegalReferenceId",
                table: "RegulatoryAlerts",
                column: "LegalReferenceId",
                principalTable: "LegalReferences",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegulatoryAlerts_LegalReferences_LegalReferenceId",
                table: "RegulatoryAlerts");

            migrationBuilder.DropTable(
                name: "DocumentTopics");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "RegulatoryUpdates");

            migrationBuilder.DropColumn(
                name: "StoredFileName",
                table: "RegulatoryUpdates");

            migrationBuilder.DropColumn(
                name: "RiskDescription",
                table: "RegulatoryAlerts");

            migrationBuilder.AlterColumn<Guid>(
                name: "LegalReferenceId",
                table: "RegulatoryAlerts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RegulatoryAlerts_LegalReferences_LegalReferenceId",
                table: "RegulatoryAlerts",
                column: "LegalReferenceId",
                principalTable: "LegalReferences",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
