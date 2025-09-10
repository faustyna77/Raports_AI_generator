using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI_Raports_Generators.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdatedAtToGeneratedEmail3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "GeneratedEmails",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "GeneratedEmails");
        }
    }
}
