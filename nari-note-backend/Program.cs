using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Application.Repository;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Infrastructure;
using NariNoteBackend.Infrastructure.Repository;
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

// Register services
builder.Services.AddScoped<CreateArticleService>();
builder.Services.AddScoped<GetArticleService>();

var app = builder.Build();

// if (app.Environment.IsDevelopment())
// {
//     app.MapOpenApi();
// }

// グローバル例外ハンドラーを最初に登録（重要）
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseHttpsRedirection();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
