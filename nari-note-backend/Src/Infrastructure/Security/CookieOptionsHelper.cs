using Microsoft.AspNetCore.Http;
using NariNoteBackend.Application.Security;

namespace NariNoteBackend.Infrastructure.Security;

public class CookieOptionsHelper : ICookieOptionsHelper
{
    readonly IWebHostEnvironment environment;

    public CookieOptionsHelper(IWebHostEnvironment environment)
    {
        this.environment = environment;
    }

    public CookieOptions CreateAuthCookieOptions(TimeSpan maxAge)
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = !environment.IsDevelopment(),
            SameSite = environment.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.Strict,
            MaxAge = maxAge,
            Path = "/"
        };
    }
}
