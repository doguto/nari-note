using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NariNoteBackend.Migrations
{
    /// <summary>
    /// 棋譜テキストの上限を DTO のバリデーション（20000 文字）に合わせて 8192 から 20000 に広げる。
    /// </summary>
    public partial class ExpandKifuTextMaxLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "KifuText",
                table: "Kifus",
                type: "character varying(20000)",
                maxLength: 20000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(8192)",
                oldMaxLength: 8192);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "KifuText",
                table: "Kifus",
                type: "character varying(8192)",
                maxLength: 8192,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20000)",
                oldMaxLength: 20000);
        }
    }
}
