using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NariNoteBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddSignUpInputAndSecurityStamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SecurityStamp",
                table: "Users",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "EmailVerifications",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "EmailVerifications",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SecurityStamp",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "EmailVerifications");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "EmailVerifications");
        }
    }
}
