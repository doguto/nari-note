using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Application.Repository;
using NariNoteBackend.Infrastructure.Helper;
using NariNoteBackend.Infrastructure.Repository;

namespace NariNoteBackend.Infrastructure.DependencyInjection;

public static class InfrastructureServiceInstaller
{
    public static void AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register DbContext
        services.AddDbContext<NariNoteDbContext>(
            options => options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
        );

        // Register repositories
        services.AddScoped<IArticleRepository, ArticleRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();

        // Register helpers
        services.AddSingleton<JwtHelper>();
    }
}
