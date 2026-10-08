using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NariNoteBackend.Infrastructure.Database;

#nullable disable

namespace NariNoteBackend.Migrations
{
    /// <summary>
    /// Users.Email の大文字小文字を区別しない一意制約と、Users.Name の一意制約を追加する。
    /// EF Core の属性では式インデックスを表現できないため、生 SQL で作成する（モデルスナップショットは変更なし）。
    /// </summary>
    [DbContext(typeof(NariNoteDbContext))]
    [Migration("20261007000000_AddUniqueLowerEmailIndexToUsers")]
    public partial class AddUniqueLowerEmailIndexToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 大文字小文字違いの重複がある場合は、データ破壊を避けるためここで中断する。
            // 検出クエリ: SELECT lower("Email"), count(*) FROM "Users" GROUP BY 1 HAVING count(*) > 1;
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM "Users" GROUP BY lower("Email") HAVING count(*) > 1
                    ) THEN
                        RAISE EXCEPTION 'Duplicate emails (case-insensitive) exist in "Users". Resolve them before applying this migration.';
                    END IF;
                END $$;
                """);

            migrationBuilder.Sql("""UPDATE "Users" SET "Email" = lower("Email") WHERE "Email" <> lower("Email");""");

            migrationBuilder.Sql("""CREATE UNIQUE INDEX ux_users_email_lower ON "Users" (lower("Email"));""");

            // 名前は大文字小文字を区別して一意にする。重複がある場合は中断する。
            // 検出クエリ: SELECT "Name", count(*) FROM "Users" GROUP BY 1 HAVING count(*) > 1;
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM "Users" GROUP BY "Name" HAVING count(*) > 1
                    ) THEN
                        RAISE EXCEPTION 'Duplicate names exist in "Users". Resolve them before applying this migration.';
                    END IF;
                END $$;
                """);

            migrationBuilder.Sql("""CREATE UNIQUE INDEX ux_users_name ON "Users" ("Name");""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP INDEX IF EXISTS ux_users_name;""");
            migrationBuilder.Sql("""DROP INDEX IF EXISTS ux_users_email_lower;""");
        }
    }
}
