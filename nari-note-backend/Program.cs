using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Application.Repository;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Infrastructure;
using NariNoteBackend.Infrastructure.Repository;
using NariNoteBackend.Infrastructure.Helper;
using NariNoteBackend.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// builder.Services.AddOpenApi();
builder.Services.AddHealthChecks().AddCheck<HealthCheckService>("health_check");
builder.Services.AddDbContext<NariNoteDbContext>(
    options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Register repositories
builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();

// Register helpers
builder.Services.AddSingleton<JwtHelper>();

// Register services
builder.Services.AddScoped<CreateArticleService>();
builder.Services.AddScoped<DeleteArticleService>();
builder.Services.AddScoped<GetArticlesByAuthorService>();
builder.Services.AddScoped<GetArticleService>();
builder.Services.AddScoped<GetUserProfileService>();
builder.Services.AddScoped<SignUpService>();
builder.Services.AddScoped<SignInService>();

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
