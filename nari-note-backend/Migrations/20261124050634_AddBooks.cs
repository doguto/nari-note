using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NariNoteBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddBooks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RootSfen = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Books_Users_UserId",
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
                    BookId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Memo = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShapeGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShapeGroups_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookPositions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BookId = table.Column<int>(type: "integer", nullable: false),
                    Sfen = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ShapeGroupId = table.Column<int>(type: "integer", nullable: false),
                    CandidatesScanned = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookPositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookPositions_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookPositions_ShapeGroups_ShapeGroupId",
                        column: x => x.ShapeGroupId,
                        principalTable: "ShapeGroups",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BookMoves",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BookId = table.Column<int>(type: "integer", nullable: false),
                    ParentPositionId = table.Column<int>(type: "integer", nullable: false),
                    ChildPositionId = table.Column<int>(type: "integer", nullable: false),
                    Usi = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    Label = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Memo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookMoves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookMoves_BookPositions_ChildPositionId",
                        column: x => x.ChildPositionId,
                        principalTable: "BookPositions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookMoves_BookPositions_ParentPositionId",
                        column: x => x.ParentPositionId,
                        principalTable: "BookPositions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookMoves_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShapeCandidates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BookId = table.Column<int>(type: "integer", nullable: false),
                    PositionAId = table.Column<int>(type: "integer", nullable: false),
                    PositionBId = table.Column<int>(type: "integer", nullable: false),
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
                        name: "FK_ShapeCandidates_BookPositions_PositionAId",
                        column: x => x.PositionAId,
                        principalTable: "BookPositions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShapeCandidates_BookPositions_PositionBId",
                        column: x => x.PositionBId,
                        principalTable: "BookPositions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShapeCandidates_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookMoves_BookId",
                table: "BookMoves",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_BookMoves_ChildPositionId",
                table: "BookMoves",
                column: "ChildPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_BookMoves_ParentPositionId_Usi",
                table: "BookMoves",
                columns: new[] { "ParentPositionId", "Usi" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookPositions_BookId_Sfen",
                table: "BookPositions",
                columns: new[] { "BookId", "Sfen" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookPositions_CandidatesScanned",
                table: "BookPositions",
                column: "CandidatesScanned");

            migrationBuilder.CreateIndex(
                name: "IX_BookPositions_ShapeGroupId",
                table: "BookPositions",
                column: "ShapeGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Books_UserId",
                table: "Books",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ShapeCandidates_BookId",
                table: "ShapeCandidates",
                column: "BookId");

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
                name: "IX_ShapeGroups_BookId",
                table: "ShapeGroups",
                column: "BookId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookMoves");

            migrationBuilder.DropTable(
                name: "ShapeCandidates");

            migrationBuilder.DropTable(
                name: "BookPositions");

            migrationBuilder.DropTable(
                name: "ShapeGroups");

            migrationBuilder.DropTable(
                name: "Books");
        }
    }
}
