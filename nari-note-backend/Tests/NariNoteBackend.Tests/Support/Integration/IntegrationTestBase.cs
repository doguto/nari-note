using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Security;
using NariNoteBackend.Infrastructure.Database;
using NariNoteBackend.Infrastructure.Outbox;

namespace NariNoteBackend.Tests.Support.Integration;

[CollectionDefinition(Name)]
public class IntegrationTestCollection : ICollectionFixture<NariNoteApiFactory>
{
    public const string Name = "Integration";
}

/// <summary>
///     HTTP 経由で API を検証する結合テストの基底クラス。
///     テストごとに DB を空にするため、必要なデータは各テストの Arrange で投入する。
/// </summary>
[Collection(IntegrationTestCollection.Name)]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected IntegrationTestBase(NariNoteApiFactory factory)
    {
        Factory = factory;
    }

    protected NariNoteApiFactory Factory { get; }

    protected TestTimeProvider TimeProvider => Factory.TimeProvider;

    // API と同じシリアライズ設定（camelCase・ValueObject 変換）でレスポンスを読む
    JsonSerializerOptions JsonOptions =>
        Factory.Services.GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions;

    public async ValueTask InitializeAsync()
    {
        await Factory.ResetAsync();
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    /// <summary>未認証のクライアント</summary>
    protected HttpClient CreateClient()
    {
        return Factory.CreateClient();
    }

    /// <summary>指定ユーザーとして認証済みのクライアント</summary>
    protected HttpClient CreateClientAs(User user)
    {
        using var scope = Factory.Services.CreateScope();
        var token = scope.ServiceProvider.GetRequiredService<IJwtHelper>().GenerateToken(user.Id, user.Name);

        return CreateClientWithToken(token);
    }

    /// <summary>指定した認証トークンを Cookie に持つクライアント</summary>
    protected HttpClient CreateClientWithToken(string token)
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("Cookie", $"authToken={token}");
        return client;
    }

    /// <summary>レスポンスの Set-Cookie から認証トークンを取り出す（無ければ null）</summary>
    protected static string? GetAuthToken(HttpResponseMessage response)
    {
        const string prefix = "authToken=";
        if (!response.Headers.TryGetValues("Set-Cookie", out var cookies)) return null;

        var cookie = cookies.FirstOrDefault(c => c.StartsWith(prefix));
        return cookie?[prefix.Length..].Split(';')[0];
    }

    /// <summary>Outbox に溜まったメッセージを配送する（本番では OutboxWorker が定期実行する処理）</summary>
    protected async Task ProcessOutboxAsync()
    {
        using var scope = Factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<OutboxProcessor>().ProcessBatchAsync();
    }

    /// <summary>テストデータを DB に投入する</summary>
    protected async Task SeedAsync(params object[] entities)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<NariNoteDbContext>();
        context.AddRange(entities);
        await context.SaveChangesAsync();
    }

    /// <summary>DB の状態を検証するためのクエリを実行する</summary>
    protected async Task<T> QueryAsync<T>(Func<NariNoteDbContext, Task<T>> query)
    {
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<NariNoteDbContext>();
        return await query(context);
    }

    protected async Task<T> ReadAsync<T>(HttpResponseMessage response)
    {
        var body = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        return body ?? throw new InvalidOperationException("Response body is empty");
    }
}
