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
                name: "ShapeGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlaybookId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Memo = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShapeGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShapeGroups_Playbooks_PlaybookId",
                        column: x => x.PlaybookId,
                        principalTable: "Playbooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlaybookPositions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlaybookId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sfen = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ShapeGroupId = table.Column<int>(type: "integer", nullable: false),
                    CandidatesScanned = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaybookPositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlaybookPositions_Playbooks_PlaybookId",
                        column: x => x.PlaybookId,
                        principalTable: "Playbooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlaybookPositions_ShapeGroups_ShapeGroupId",
                        column: x => x.ShapeGroupId,
                        principalTable: "ShapeGroups",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PlaybookMoves",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlaybookId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentPositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChildPositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Usi = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Label = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Memo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Condition = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaybookMoves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlaybookMoves_PlaybookPositions_ChildPositionId",
                        column: x => x.ChildPositionId,
                        principalTable: "PlaybookPositions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlaybookMoves_PlaybookPositions_ParentPositionId",
                        column: x => x.ParentPositionId,
                        principalTable: "PlaybookPositions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlaybookMoves_Playbooks_PlaybookId",
                        column: x => x.PlaybookId,
                        principalTable: "Playbooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShapeCandidates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlaybookId = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionAId = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionBId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Verdict = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Delta = table.Column<int>(type: "integer", nullable: true),
                    ConfirmedDepth = table.Column<int>(type: "integer", nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Decision = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShapeCandidates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShapeCandidates_PlaybookPositions_PositionAId",
                        column: x => x.PositionAId,
                        principalTable: "PlaybookPositions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShapeCandidates_PlaybookPositions_PositionBId",
                        column: x => x.PositionBId,
                        principalTable: "PlaybookPositions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShapeCandidates_Playbooks_PlaybookId",
                        column: x => x.PlaybookId,
                        principalTable: "Playbooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlaybookMoves_ChildPositionId",
                table: "PlaybookMoves",
                column: "ChildPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaybookMoves_ParentPositionId_Usi",
                table: "PlaybookMoves",
                columns: new[] { "ParentPositionId", "Usi" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlaybookMoves_PlaybookId",
                table: "PlaybookMoves",
                column: "PlaybookId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaybookPositions_CandidatesScanned",
                table: "PlaybookPositions",
                column: "CandidatesScanned");

            migrationBuilder.CreateIndex(
                name: "IX_PlaybookPositions_PlaybookId_Sfen",
                table: "PlaybookPositions",
                columns: new[] { "PlaybookId", "Sfen" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlaybookPositions_ShapeGroupId",
                table: "PlaybookPositions",
                column: "ShapeGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Playbooks_UserId",
                table: "Playbooks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ShapeCandidates_PlaybookId",
                table: "ShapeCandidates",
                column: "PlaybookId");

            migrationBuilder.CreateIndex(
                name: "IX_ShapeCandidates_PositionAId_PositionBId",
                table: "ShapeCandidates",
                columns: new[] { "PositionAId", "PositionBId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShapeCandidates_PositionBId",
                table: "ShapeCandidates",
                column: "PositionBId");

            migrationBuilder.CreateIndex(
                name: "IX_ShapeCandidates_Status",
                table: "ShapeCandidates",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ShapeGroups_PlaybookId",
                table: "ShapeGroups",
                column: "PlaybookId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlaybookMoves");

            migrationBuilder.DropTable(
                name: "ShapeCandidates");

            migrationBuilder.DropTable(
                name: "PlaybookPositions");

            migrationBuilder.DropTable(
                name: "ShapeGroups");

            migrationBuilder.DropTable(
                name: "Playbooks");
        }
    }
}
