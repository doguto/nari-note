using Amazon.Lambda;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.Gateway;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.Security;
using NariNoteBackend.Infrastructure.Database;
using NariNoteBackend.Infrastructure.Gateway;
using NariNoteBackend.Infrastructure.Job;
using NariNoteBackend.Infrastructure.Repository;
using NariNoteBackend.Infrastructure.Security;
using Npgsql;
using Resend;

namespace NariNoteBackend.Infrastructure;

public static class InfrastructureServiceInstaller
{
    static readonly TimeSpan ExternalRequestTimeout = TimeSpan.FromSeconds(10);
    static readonly TimeSpan ShogiEngineTimeout = TimeSpan.FromMinutes(5);

    // 本番 PostgreSQL の max_connections (infrastruture/terraform/prod/app/server/postgresql.conf) より小さく保つこと
    // 上限を超えた分は DB に拒否されずプール内で Timeout まで待機させる
    const int DbMaxPoolSize = 20;
    const string DbApplicationName = "nari-note-backend";

    public static void AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment env
    )
    {
        // Register DbContext
        // GetConnectionString("DefaultConnection") は ConnectionStrings__DefaultConnection 環境変数 / appsettings を参照
        // 取得できない場合は SSM から読み込んだ個別キーでフォールバック
        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? $"Host={configuration["host"]};" +
            $"Port={configuration["port"]};" +
            $"Database={configuration["name"]};" +
            $"Username={configuration["username"]};" +
            $"Password={configuration["password"]}";
        var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            MaxPoolSize = DbMaxPoolSize,
            ApplicationName = DbApplicationName
        };
        // TransactionMiddleware がリクエスト全体をトランザクション単位にしているため、EnableRetryOnFailure は使用しない
        services.AddDbContext<NariNoteDbContext>(
            options => options.UseNpgsql(connectionStringBuilder.ConnectionString)
        );

        // Register repositories
        services.AddScoped<IEmailVerificationRepository, EmailVerificationRepository>();
        services.AddScoped<IArticleRepository, ArticleRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<ILikeRepository, LikeRepository>();
        services.AddScoped<ICourseLikeRepository, CourseLikeRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<IFollowRepository, FollowRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<IKifuRepository, KifuRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
        services.AddScoped<IBookRepository, BookRepository>();

        services.AddScoped<OutboxProcessor>();
        services.AddHostedService<OutboxWorker>();

        services.AddScoped<UnverifiedUserDeletionProcessor>();
        services.AddHostedService<UnverifiedUserDeletionWorker>();

        services.AddScoped<ShapeJudgeProcessor>();
        services.AddHostedService<ShapeJudgeWorker>();

        // Register helpers
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IJwtHelper, JwtHelper>();
        services.AddScoped<ICookieOptionsHelper, CookieOptionsHelper>();

        // Register gateways
        services.AddOptions();
        services.AddHttpClient<ResendClient>(client => client.Timeout = ExternalRequestTimeout);
        services.Configure<ResendClientOptions>(o => { o.ApiToken = configuration["resend_api_token"]!; });
        services.AddTransient<IResend, ResendClient>();
        services.AddScoped<IEmailHelper, ResendEmailHelper>();
        var frontendBaseUrl = configuration["Frontend:BaseUrl"]
            ?? throw new InvalidOperationException("Frontend:BaseUrl が設定されていません");
        services.AddSingleton(new EmailMessageStore(frontendBaseUrl));
        services.AddHttpClient<IDiscordNotifier, DiscordWebhookNotifier>(client => client.Timeout = ExternalRequestTimeout);
        // エンジンは API サーバーとは別に動かす。本番は Lambda、ローカルは nari-note-engine の HTTP サーバーを呼び、どちらも無い環境では解析と判定を無効にする
        var shogiEngineFunctionName = configuration["ShogiEngine:FunctionName"];
        var shogiEngineBaseUrl = configuration["ShogiEngine:BaseUrl"];
        if (!string.IsNullOrWhiteSpace(shogiEngineFunctionName))
        {
            // 判定は 1 件で数十秒かかる
            services.AddSingleton<IAmazonLambda>(_ => new AmazonLambdaClient(new AmazonLambdaConfig { Timeout = ShogiEngineTimeout }));
            services.AddSingleton<IShogiEngineGateway>(sp => new LambdaShogiEngineGateway(
                sp.GetRequiredService<IAmazonLambda>(),
                shogiEngineFunctionName
            ));
        }
        else if (!string.IsNullOrWhiteSpace(shogiEngineBaseUrl))
        {
            services.AddHttpClient<IShogiEngineGateway, HttpShogiEngineGateway>(client =>
            {
                client.BaseAddress = new Uri(shogiEngineBaseUrl.TrimEnd('/') + "/");
                client.Timeout = ShogiEngineTimeout;
            });
        }
        else
        {
            services.AddSingleton<IShogiEngineGateway, DisabledShogiEngineGateway>();
        }

        if (env.IsDevelopment())
            services.AddScoped<IImageStorageGateway, LocalImageStorageGateway>();
        else
            services.AddScoped<IImageStorageGateway, S3ImageStorageGateway>();
    }
}
