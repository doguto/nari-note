using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Application.Repository;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Infrastructure;
using NariNoteBackend.Infrastructure.Repository;

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
builder.Services.AddScoped<DeleteArticleService>();

var app = builder.Build();

// if (app.Environment.IsDevelopment())
// {
//     app.MapOpenApi();
// }

app.UseHttpsRedirection();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
