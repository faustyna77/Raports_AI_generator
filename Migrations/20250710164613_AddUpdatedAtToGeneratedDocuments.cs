using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI_Raports_Generators.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdatedAtToGeneratedDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "GeneratedDocuments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedDocuments_UserId",
                table: "GeneratedDocuments",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GeneratedDocuments_AspNetUsers_UserId",
                table: "GeneratedDocuments",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GeneratedDocuments_AspNetUsers_UserId",
                table: "GeneratedDocuments");

            migrationBuilder.DropIndex(
                name: "IX_GeneratedDocuments_UserId",
                table: "GeneratedDocuments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "GeneratedDocuments");
        }
    }
}
