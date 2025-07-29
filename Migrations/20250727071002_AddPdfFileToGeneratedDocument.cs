using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI_Raports_Generators.Migrations
{
    /// <inheritdoc />
    public partial class AddPdfFileToGeneratedDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "PdfFile",
                table: "GeneratedDocuments",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PdfFile",
                table: "GeneratedDocuments");
        }
    }
}
