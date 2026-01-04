using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Application.Security;
using NariNoteBackend.Infrastructure.Repository;
using NariNoteBackend.Infrastructure.Security;

namespace NariNoteBackend.Infrastructure;

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
        services.AddScoped<ILikeRepository, LikeRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        
        // Register helpers
        services.AddScoped<IJwtHelper, JwtHelper>();
    }
}
