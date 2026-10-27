using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace NariNoteBackend.Infrastructure.Database;

/// <summary>
///     実行計画やパフォーマンスを本番の将来規模で確認するための大量ダミーデータを投入します。
///     EF Core 経由では件数に対して遅すぎるため、generate_series で DB 側に生成させます。
/// </summary>
public static class BulkDataSeeder
{
    const string MarkerEmail = "bulk-user1@example.com";

    const int UserCount = 10_000;
    const int ArticleCount = 100_000;
    const int TagCount = 500;
    const int LikeCount = 1_000_000;
    const int CommentCount = 300_000;
    const int FollowCount = 100_000;

    static readonly TimeSpan CommandTimeout = TimeSpan.FromMinutes(10);

    // 著者・タグ・いいね等は power(random(), k) で偏らせ、一部のユーザー/記事にデータが集中する実運用に近い分布にする
    // ID はアプリと同じく UUIDv7 とし、主キーインデックスの物理的な並びを本番に揃える
    static readonly string SeedSql = $$"""
        CREATE FUNCTION pg_temp.uuid_v7(ts timestamptz) RETURNS uuid AS $$
            SELECT encode(
                set_bit(set_bit(
                    overlay(uuid_send(gen_random_uuid())
                            PLACING substring(int8send(floor(extract(epoch FROM ts) * 1000)::bigint) FROM 3)
                            FROM 1 FOR 6),
                    52, 1), 53, 1),
                'hex')::uuid;
        $$ LANGUAGE sql VOLATILE;

        CREATE TEMP TABLE bulk_users ON COMMIT DROP AS
        SELECT g AS n,
               pg_temp.uuid_v7(now() - ({{UserCount}} - g) * interval '1 hour') AS id,
               now() - ({{UserCount}} - g) * interval '1 hour' AS created_at
        FROM generate_series(1, {{UserCount}}) g;
        CREATE INDEX ON bulk_users (n);

        INSERT INTO "Users" ("Id", "Name", "Email", "PasswordHash", "IsEmailVerified", "CreatedAt", "UpdatedAt")
        SELECT id, 'ダミーユーザー' || n, 'bulk-user' || n || '@example.com', @passwordHash, true, created_at, created_at
        FROM bulk_users;

        -- 10 分おきに作成された約 2 年分の記事。1 割は下書き (PublishedAt = NULL)
        CREATE TEMP TABLE bulk_articles ON COMMIT DROP AS
        SELECT g AS n,
               pg_temp.uuid_v7(now() - ({{ArticleCount}} - g) * interval '10 minutes') AS id,
               1 + floor(power(random(), 3) * {{UserCount}})::int AS author_n,
               now() - ({{ArticleCount}} - g) * interval '10 minutes' AS created_at,
               g % 10 = 0 AS is_draft
        FROM generate_series(1, {{ArticleCount}}) g;
        CREATE INDEX ON bulk_articles (n);

        INSERT INTO "Articles" ("Id", "AuthorId", "Title", "Body", "CreatedAt", "UpdatedAt", "PublishedAt")
        SELECT a.id,
               u.id,
               CASE WHEN a.n % 7 = 0 THEN '将棋の研究ノート ' || a.n ELSE 'ダミー記事 ' || a.n END,
               '## ダミー記事 ' || a.n || E'\n\n' || repeat(E'居飛車と振り飛車の序盤から終盤までを解説します。\n戦型ごとの考え方を順に見ていきます。\n\n', 15),
               a.created_at,
               a.created_at,
               CASE WHEN a.is_draft THEN NULL ELSE a.created_at + interval '1 hour' END
        FROM bulk_articles a
        JOIN bulk_users u ON u.n = a.author_n;

        CREATE TEMP TABLE bulk_tags ON COMMIT DROP AS
        SELECT g AS n, pg_temp.uuid_v7(now()) AS id
        FROM generate_series(1, {{TagCount}}) g;
        CREATE INDEX ON bulk_tags (n);

        INSERT INTO "Tags" ("Id", "Name", "CreatedAt", "UpdatedAt")
        SELECT id, 'ダミータグ' || n, now(), now()
        FROM bulk_tags;

        -- 記事 1 件あたり最大 3 タグ
        -- random() を JOIN 条件に書くと記事ごとに評価されないため、サブクエリで先に乱数を確定させる
        INSERT INTO "ArticleTags" ("ArticleId", "TagId", "CreatedAt", "UpdatedAt")
        SELECT s.id, t.id, s.created_at, s.created_at
        FROM (
            SELECT a.id, a.created_at, 1 + floor(power(random(), 2) * {{TagCount}})::int AS tag_n
            FROM bulk_articles a
            CROSS JOIN generate_series(1, 3)
        ) s
        JOIN bulk_tags t ON t.n = s.tag_n
        ON CONFLICT DO NOTHING;

        -- いいね・コメントは新しい記事ほど集中させる
        INSERT INTO "Likes" ("UserId", "ArticleId", "CreatedAt", "UpdatedAt")
        SELECT u.id, a.id, a.created_at + random() * (now() - a.created_at), now()
        FROM (
            SELECT 1 + floor(random() * {{UserCount}})::int AS user_n,
                   1 + floor((1 - power(random(), 3)) * {{ArticleCount}})::int AS article_n
            FROM generate_series(1, {{LikeCount}})
        ) s
        JOIN bulk_users u ON u.n = s.user_n
        JOIN bulk_articles a ON a.n = s.article_n
        ON CONFLICT DO NOTHING;

        INSERT INTO "Comments" ("ArticleId", "UserId", "Message", "CreatedAt", "UpdatedAt")
        SELECT a.id, u.id, 'ダミーコメント ' || s.g, a.created_at + random() * (now() - a.created_at), now()
        FROM (
            SELECT g,
                   1 + floor(random() * {{UserCount}})::int AS user_n,
                   1 + floor((1 - power(random(), 3)) * {{ArticleCount}})::int AS article_n
            FROM generate_series(1, {{CommentCount}}) g
        ) s
        JOIN bulk_users u ON u.n = s.user_n
        JOIN bulk_articles a ON a.n = s.article_n;

        INSERT INTO "Follows" ("FollowerId", "FollowingId", "CreatedAt", "UpdatedAt")
        SELECT f.id, fg.id, now(), now()
        FROM (
            SELECT 1 + floor(random() * {{UserCount}})::int AS follower_n,
                   1 + floor(power(random(), 3) * {{UserCount}})::int AS following_n
            FROM generate_series(1, {{FollowCount}})
        ) s
        JOIN bulk_users f ON f.n = s.follower_n
        JOIN bulk_users fg ON fg.n = s.following_n
        WHERE s.follower_n <> s.following_n
        ON CONFLICT DO NOTHING;
        """;

    public static async Task SeedAsync(NariNoteDbContext context)
    {
        if (await context.Users.AnyAsync(u => u.Email == MarkerEmail)) return;

        var stopwatch = Stopwatch.StartNew();
        context.Database.SetCommandTimeout(CommandTimeout);

        await using (var transaction = await context.Database.BeginTransactionAsync())
        {
            var passwordHash = new NpgsqlParameter("passwordHash", BCrypt.Net.BCrypt.HashPassword("password123"));
            await context.Database.ExecuteSqlRawAsync(SeedSql, passwordHash);
            await transaction.CommitAsync();
        }

        // VACUUM はトランザクション内で実行できない。可視性マップを作成し Index Only Scan を選択可能にする
        await context.Database.ExecuteSqlRawAsync("VACUUM ANALYZE");

        Console.WriteLine($"✅ 大量シードデータの投入が完了しました (ElapsedSec={stopwatch.Elapsed.TotalSeconds:F1})");
    }
}
