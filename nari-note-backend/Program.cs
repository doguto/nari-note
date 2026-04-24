using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Application;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Infrastructure;
using NariNoteBackend.Infrastructure.Database;
using NariNoteBackend.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

if (!builder.Environment.IsDevelopment())
{
    builder.Configuration.AddSystemsManager("/nari-note/app", false);
    builder.Configuration.AddSystemsManager("/nari-note/db", false);
}
else
{
    builder.Configuration.AddJsonFile("secret.json", true, false);
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
builder.Host.UseSerilog((context, configuration) => { configuration.ReadFrom.Configuration(context.Configuration); });

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

// CORSミドルウェアを最初に登録（preflightリクエスト対応のため）
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
