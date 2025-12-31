using NariNoteBackend.Application.DependencyInjection;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Infrastructure.DependencyInjection;
using NariNoteBackend.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// builder.Services.AddOpenApi();
builder.Services.AddHealthChecks().AddCheck<HealthCheckService>("health_check");

// Register infrastructure services (DbContext, Repositories, Helpers)
builder.Services.AddInfrastructureServices(builder.Configuration);

// Register application services
builder.Services.AddApplicationServices();

var app = builder.Build();

// if (app.Environment.IsDevelopment())
// {
//     app.MapOpenApi();
// }

// グローバル例外ハンドラーを最初に登録（重要）
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// 認証ミドルウェアを登録
app.UseMiddleware<AuthenticationMiddleware>();

app.UseHttpsRedirection();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
