using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI_Raports_Generators.Migrations
{
    /// <inheritdoc />
    public partial class AddPdfAndAiFieldsToDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AIGeneratedContent",
                table: "Documents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "PdfFile",
                table: "Documents",
                type: "bytea",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AIGeneratedContent",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "PdfFile",
                table: "Documents");
        }
    }
}
