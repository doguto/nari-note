using NariNoteBackend.Application.Service;

namespace NariNoteBackend.Application;

public static class ApplicationServiceInstaller
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        // Register services
        services.AddScoped<CreateArticleService>();
        services.AddScoped<UpdateArticleService>();
        services.AddScoped<DeleteArticleService>();
        services.AddScoped<GetArticlesService>();
        services.AddScoped<GetArticlesByAuthorService>();
        services.AddScoped<GetArticlesByTagService>();
        services.AddScoped<GetArticleService>();
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
        services.AddScoped<ToggleFollowService>();
        services.AddScoped<CreateCommentService>();
        services.AddScoped<GetFollowersService>();
        services.AddScoped<GetFollowingsService>();
        services.AddScoped<GetLikedArticlesService>();
        services.AddScoped<GetTagsService>();
    }
}
