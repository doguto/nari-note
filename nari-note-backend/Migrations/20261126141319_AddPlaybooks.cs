using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NariNoteBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddPlaybooks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Playbooks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RootSfen = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Playbooks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Playbooks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlaybookNodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlaybookId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Memo = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaybookNodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlaybookNodes_Playbooks_PlaybookId",
                        column: x => x.PlaybookId,
                        principalTable: "Playbooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlaybookBoards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlaybookId = table.Column<Guid>(type: "uuid", nullable: false),
                    NodeId = table.Column<int>(type: "integer", nullable: true),
                    Sfen = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaybookBoards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlaybookBoards_PlaybookNodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "PlaybookNodes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PlaybookBoards_Playbooks_PlaybookId",
                        column: x => x.PlaybookId,
                        principalTable: "Playbooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlaybookBoardTransitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlaybookId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromBoardId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToBoardId = table.Column<Guid>(type: "uuid", nullable: false),
                    Usi = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Condition = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaybookBoardTransitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlaybookBoardTransitions_PlaybookBoards_FromBoardId",
                        column: x => x.FromBoardId,
                        principalTable: "PlaybookBoards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlaybookBoardTransitions_PlaybookBoards_ToBoardId",
                        column: x => x.ToBoardId,
                        principalTable: "PlaybookBoards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlaybookBoardTransitions_Playbooks_PlaybookId",
                        column: x => x.PlaybookId,
                        principalTable: "Playbooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlaybookBoards_NodeId",
                table: "PlaybookBoards",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaybookBoards_PlaybookId_Sfen",
                table: "PlaybookBoards",
                columns: new[] { "PlaybookId", "Sfen" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlaybookBoardTransitions_FromBoardId_ToBoardId",
                table: "PlaybookBoardTransitions",
                columns: new[] { "FromBoardId", "ToBoardId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlaybookBoardTransitions_PlaybookId",
                table: "PlaybookBoardTransitions",
                column: "PlaybookId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaybookBoardTransitions_ToBoardId",
                table: "PlaybookBoardTransitions",
                column: "ToBoardId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaybookNodes_PlaybookId",
                table: "PlaybookNodes",
                column: "PlaybookId");

            migrationBuilder.CreateIndex(
                name: "IX_Playbooks_UserId",
                table: "Playbooks",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlaybookBoardTransitions");

            migrationBuilder.DropTable(
                name: "PlaybookBoards");

            migrationBuilder.DropTable(
                name: "PlaybookNodes");

            migrationBuilder.DropTable(
                name: "Playbooks");
        }
    }
}
