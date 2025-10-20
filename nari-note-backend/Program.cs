using Microsoft.EntityFrameworkCore;
using nari_note_backend.Infrastructure;
using NariNoteBackend.Src.Application.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// builder.Services.AddOpenApi();
builder.Services.AddHealthChecks().AddCheck<HealthCheckService>("health_check");
builder.Services.AddDbContext<NariNoteDbContext>(
    options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

var app = builder.Build();

// if (app.Environment.IsDevelopment())
// {
//     app.MapOpenApi();
// }

app.UseHttpsRedirection();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
