using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Infrastructure.Database;
using NariNoteBackend.Tests.Support;
using NariNoteBackend.Tests.Support.Builder;
using Testcontainers.PostgreSql;

namespace NariNoteBackend.Tests.Migrations;

/// <summary>
///     既存データがある状態で UseIdentityForIntIds を適用し、IDENTITY の採番位置が既存の ID と衝突しないことを検証する。
///     途中のマイグレーションまでしか適用しない DB が必要なため、結合テストとは別のコンテナを使う。
/// </summary>
public class UseIdentityForIntIdsTest : IAsyncLifetime
{
    const string PreviousMigration = "20261009132911_AddOutboxMessages";

    readonly PostgreSqlContainer postgres = new PostgreSqlBuilder("postgres:17.11-alpine3.24").Build();

    public async ValueTask InitializeAsync()
    {
        await this.postgres.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await this.postgres.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task 適用後は既存の最大IDの次から採番され空のテーブルは1から採番される()
    {
        var author = new UserBuilder().Build();
        var readers = Enumerable.Range(0, 3).Select(_ => new UserBuilder().Build()).ToArray();
        var article = new ArticleBuilder(author).Build();

        await using (var context = CreateContext())
        {
            await context.GetService<IMigrator>().MigrateAsync(PreviousMigration);

            // 移行前はアプリ側で採番していたため、ID を明示して投入する
            context.AddRange(author, article);
            context.AddRange(readers);
            context.Likes.AddRange(
                readers.Select((r, i) =>
                {
                    var like = TestEntity.Like(r, article);
                    like.Id = LikeId.From(i + 1);
                    return like;
                })
            );
            await context.SaveChangesAsync();

            await context.Database.MigrateAsync();
        }

        await using (var context = CreateContext())
        {
            var like = TestEntity.Like(author, article);
            var comment = TestEntity.Comment(author, article, "移行後のコメント");
            context.AddRange(like, comment);
            await context.SaveChangesAsync();

            Assert.Equal(LikeId.From(readers.Length + 1), like.Id);
            Assert.Equal(CommentId.From(1), comment.Id);
        }
    }

    NariNoteDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<NariNoteDbContext>()
            .UseNpgsql(this.postgres.GetConnectionString())
            .Options;
        return new NariNoteDbContext(options, new TestTimeProvider());
    }
}
