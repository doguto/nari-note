using NariNoteBackend.Application.Service;

namespace NariNoteBackend.Application.DependencyInjection;

public static class ApplicationServiceInstaller
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register services
        services.AddScoped<CreateArticleService>();
        services.AddScoped<DeleteArticleService>();
        services.AddScoped<GetArticlesByAuthorService>();
        services.AddScoped<GetArticleService>();
        services.AddScoped<GetUserProfileService>();
        services.AddScoped<SignUpService>();
        services.AddScoped<SignInService>();
        services.AddScoped<HealthCheckService>();

        return services;
    }
}
