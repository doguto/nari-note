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
        // GetConnectionString("DefaultConnection") は ConnectionStrings__DefaultConnection 環境変数 / appsettings を参照
        // 取得できない場合は SSM から読み込んだ個別キーでフォールバック
        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? $"Host={configuration["host"]};" +
               $"Port={configuration["port"]};" +
               $"Database={configuration["name"]};" +
               $"Username={configuration["username"]};" +
               $"Password={configuration["password"]}";
        services.AddDbContext<NariNoteDbContext>(
            options => options.UseNpgsql(connectionString)
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
