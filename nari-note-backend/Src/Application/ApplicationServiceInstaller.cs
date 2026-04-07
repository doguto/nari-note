using NariNoteBackend.Application.Service;

namespace NariNoteBackend.Application;

public static class ApplicationServiceInstaller
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        // Register services
        services.AddScoped<CreateArticleService>();
        services.AddScoped<CreateCourseService>();
        services.AddScoped<UpdateArticleService>();
        services.AddScoped<UpdateCourseService>();
        services.AddScoped<DeleteArticleService>();
        services.AddScoped<DeleteCourseService>();
        services.AddScoped<GetArticlesService>();
        services.AddScoped<GetCoursesService>();
        services.AddScoped<GetArticlesByAuthorService>();
        services.AddScoped<GetCoursesByAuthorService>();
        services.AddScoped<GetArticlesByTagService>();
        services.AddScoped<GetArticleContentService>();
        services.AddScoped<GetCourseContentService>();
        services.AddScoped<GetUserProfileService>();
        services.AddScoped<UpdateUserProfileService>();
        services.AddScoped<SignUpService>();
        services.AddScoped<SignInService>();
        services.AddScoped<GetCurrentUserService>();
        services.AddScoped<LogoutService>();
        services.AddScoped<HealthCheckService>();
        services.AddScoped<ToggleLikeService>();
        services.AddScoped<GetDraftArticlesService>();
        services.AddScoped<SearchArticlesService>();
        services.AddScoped<SearchCoursesService>();
        services.AddScoped<ToggleFollowService>();
        services.AddScoped<CreateCommentService>();
        services.AddScoped<GetFollowersService>();
        services.AddScoped<GetFollowingsService>();
        services.AddScoped<GetLikedArticlesService>();
        services.AddScoped<GetPopularTagsService>();
    }
}
