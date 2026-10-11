using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using NariNoteBackend.Domain.Gateway;
using NariNoteBackend.Infrastructure.Outbox;
using NariNoteBackend.Tests.Support.Fake;
using Npgsql;
using Respawn;
using Respawn.Graph;
using Testcontainers.PostgreSql;

namespace NariNoteBackend.Tests.Support.Integration;

/// <summary>
///     実 PostgreSQL（Testcontainers）に接続した API を起動する。
///     起動コストが高いため、結合テスト全体で 1 インスタンスを共有する。
/// </summary>
public class NariNoteApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // docker-compose.yml の db と同じイメージを使用する
    readonly PostgreSqlContainer postgres = new PostgreSqlBuilder("postgres:16.10-alpine3.22").Build();

    Respawner? respawner;

    public TestTimeProvider TimeProvider { get; } = new();
    public FakeEmailHelper EmailHelper { get; } = new();
    public FakeDiscordNotifier DiscordNotifier { get; } = new();
    public FakeImageStorageGateway ImageStorageGateway { get; } = new();

    public async ValueTask InitializeAsync()
    {
        await this.postgres.StartAsync();

        // ホストを起動し、Program.cs のマイグレーションを適用させる
        _ = Services;

        await using var connection = new NpgsqlConnection(this.postgres.GetConnectionString());
        await connection.OpenAsync();
        this.respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            TablesToIgnore = [new Table("__EFMigrationsHistory")]
        });
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await this.postgres.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     DB の全データ・時計・Fake の記録を初期状態に戻す。
    /// </summary>
    public async Task ResetAsync()
    {
        await using var connection = new NpgsqlConnection(this.postgres.GetConnectionString());
        await connection.OpenAsync();
        await this.respawner!.ResetAsync(connection);

        this.TimeProvider.Reset();
        this.EmailHelper.SentMessages.Clear();
        this.DiscordNotifier.Messages.Clear();
        this.DiscordNotifier.Embeds.Clear();
        this.ImageStorageGateway.UploadedUserIds.Clear();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Program.cs は Build 前に設定値を読むため、ConfigureAppConfiguration ではなく UseSetting で渡す
        builder.UseSetting("ConnectionStrings:DefaultConnection", this.postgres.GetConnectionString());
        builder.UseSetting("sentry_dsn", "");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(this.TimeProvider);

            // 外部サービスへは接続しない
            services.RemoveAll<IEmailHelper>();
            services.AddSingleton<IEmailHelper>(this.EmailHelper);
            services.RemoveAll<IDiscordNotifier>();
            services.AddSingleton<IDiscordNotifier>(this.DiscordNotifier);
            services.RemoveAll<IImageStorageGateway>();
            services.AddSingleton<IImageStorageGateway>(this.ImageStorageGateway);

            // Outbox はバックグラウンドで処理させず、必要なテストが OutboxProcessor を直接呼ぶ
            var outboxWorker = services.Single(d =>
                d.ServiceType == typeof(IHostedService) && d.ImplementationType == typeof(OutboxWorker)
            );
            services.Remove(outboxWorker);
        });
    }
}
