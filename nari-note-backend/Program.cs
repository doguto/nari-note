using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Application;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Infrastructure;
using NariNoteBackend.Infrastructure.Database;
using NariNoteBackend.Infrastructure.Outbox;
using NariNoteBackend.Middleware;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("secret.json", true, false);
}
// テスト時は WebApplicationFactory から設定を注入するため、SSM は参照しない
else if (!builder.Environment.IsEnvironment("Testing"))
{
    var appName = Environment.GetEnvironmentVariable("APP_NAME") ?? "nari-note";
    builder.Configuration.AddSystemsManager($"/{appName}/app", false);
    builder.Configuration.AddSystemsManager($"/{appName}/db", false);
}

// Sentry設定
builder.WebHost.UseSentry(o =>
{
    o.Dsn = builder.Configuration["sentry_dsn"];
    o.Debug = builder.Environment.IsDevelopment();
});

// CORS設定
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://nari-note.com")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Serilogの設定をsettings.jsonから取り込み
builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration)
                 // Outbox のポーリングは数秒おきに走るため、その SQL ログ等は出力しない（Warning 以上は残す）
                 .Filter.ByExcluding(logEvent =>
                     logEvent.Level < LogEventLevel.Warning
                     && logEvent.Properties.ContainsKey(OutboxProcessor.PollingLogProperty));
});

// ValueObject の設定
builder.Services.AddControllers(options => { options.ModelBinderProviders.Insert(0, new ValueObjectModelBinderProvider()); })
       .AddJsonOptions(options =>
       {
           options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
           options.JsonSerializerOptions.Converters.Add(new ValueObjectJsonConverterFactory());
       });

builder.Services.AddHealthChecks().AddCheck<HealthCheckService>("health_check");
builder.Services.AddInfrastructureServices(builder.Configuration, builder.Environment);
builder.Services.AddApplicationServices();

var app = builder.Build();

// マイグレーション適用
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<NariNoteDbContext>();
    await context.Database.MigrateAsync();
}

// 開発環境でのシードデータ投入
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<NariNoteDbContext>();
    await DataSeeder.SeedAsync(context);
}

// [NoTransaction] 等のエンドポイント情報を後続ミドルウェアで参照するため先頭に置く
app.UseRouting();

// CORSミドルウェアを登録（preflightリクエスト対応のため）
app.UseCors();

// SerilogによるAPIリクエストのログ出力を設定
app.UseSerilogRequestLogging();

// グローバル例外ハンドラーを最初に登録
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// JWT認証ミドルウェアを登録
app.UseMiddleware<JwtAuthenticationMiddleware>();

// 書き込みリクエストのDBトランザクション管理
app.UseMiddleware<TransactionMiddleware>();

app.UseHttpsRedirection();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// 結合テストの WebApplicationFactory<Program> から参照できるよう公開する
public partial class Program;
