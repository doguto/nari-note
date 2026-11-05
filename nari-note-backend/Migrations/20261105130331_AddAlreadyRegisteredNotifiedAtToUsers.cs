using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NariNoteBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddAlreadyRegisteredNotifiedAtToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AlreadyRegisteredNotifiedAt",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlreadyRegisteredNotifiedAt",
                table: "Users");
        }
    }
}
