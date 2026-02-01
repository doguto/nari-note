using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NariNoteBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddPublishedAtToCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                table: "Courses",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "Courses");
        }
    }
}
