using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Infrastructure.Cleanup;
using NariNoteBackend.Tests.Support;
using NariNoteBackend.Tests.Support.Builder;
using NariNoteBackend.Tests.Support.Integration;

namespace NariNoteBackend.Tests.Infrastructure;

// 削除条件のクエリを検証するため、実 DB に接続して UnverifiedUserCleanupProcessor を実行する。
public class UnverifiedUserCleanupProcessorTest : IntegrationTestBase
{
    static readonly DateTime Now = TestTimeProvider.DefaultUtcNow;
    static readonly DateTime Expired = Now - UnverifiedUserCleanupProcessor.Retention;

    public UnverifiedUserCleanupProcessorTest(NariNoteApiFactory factory) : base(factory)
    {
    }

    async Task<int> ProcessAsync()
    {
        using var scope = Factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<UnverifiedUserCleanupProcessor>().ProcessAsync();
    }

    async Task<bool> ExistsAsync(User user)
    {
        return await QueryAsync(db => db.Users.AnyAsync(u => u.Id == user.Id));
    }

    [Fact]
    public async Task 作成から保持期間を過ぎた未認証ユーザーは確認トークンごと削除される()
    {
        var user = new UserBuilder().EmailUnverified().CreatedAt(Expired.AddTicks(-1)).Build();
        await SeedAsync(user, TestEntity.EmailVerification(user, "expired-token", Expired.AddHours(24)));

        var deleted = await ProcessAsync();

        Assert.Equal(1, deleted);
        Assert.False(await ExistsAsync(user));
        Assert.Equal(0, await QueryAsync(db => db.EmailVerifications.CountAsync()));
    }

    [Fact]
    public async Task 作成から保持期間が経過していない未認証ユーザーは削除されない()
    {
        var user = new UserBuilder().EmailUnverified().CreatedAt(Expired).Build();
        await SeedAsync(user);

        Assert.Equal(0, await ProcessAsync());

        Assert.True(await ExistsAsync(user));
    }

    [Fact]
    public async Task 有効な確認トークンを持つ未認証ユーザーは保持期間を過ぎていても削除されない()
    {
        // 保持期間の経過直前に再度サインアップし、確認メールを受け取った直後のユーザー
        var user = new UserBuilder().EmailUnverified().CreatedAt(Expired.AddDays(-1)).Build();
        await SeedAsync(user, TestEntity.EmailVerification(user, "valid-token", Now));

        Assert.Equal(0, await ProcessAsync());

        Assert.True(await ExistsAsync(user));
    }

    [Fact]
    public async Task 認証済みのユーザーは削除されない()
    {
        var user = new UserBuilder().CreatedAt(Expired.AddDays(-1)).Build();
        await SeedAsync(user);

        Assert.Equal(0, await ProcessAsync());

        Assert.True(await ExistsAsync(user));
    }
}
