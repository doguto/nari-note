using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.Security;
using NariNoteBackend.Infrastructure.Database;
using NariNoteBackend.Infrastructure.Repository;
using NariNoteBackend.Infrastructure.Security;

namespace NariNoteBackend.Infrastructure;

public static class InfrastructureServiceInstaller
{
    public static void AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Register DbContext
        services.AddDbContext<NariNoteDbContext>(
            options => options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
        );

        // Register repositories
        services.AddScoped<IArticleRepository, ArticleRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<ILikeRepository, LikeRepository>();
        services.AddScoped<ICourseLikeRepository, CourseLikeRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<IFollowRepository, FollowRepository>();
        services.AddScoped<ITagRepository, TagRepository>();

        // Register helpers
        services.AddScoped<IJwtHelper, JwtHelper>();
        services.AddScoped<ICookieOptionsHelper, CookieOptionsHelper>();
    }
}
