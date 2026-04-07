using Microsoft.AspNetCore.Http;
using NariNoteBackend.Application.Dto.Request;

namespace NariNoteBackend.Application.Service;

public class LogoutService
{
    public Task ExecuteAsync(LogoutRequest request, HttpResponse response)
    {
        // Cookieを削除（path指定で確実に削除）
        response.Cookies.Delete("authToken", new CookieOptions
        {
            Path = "/"
        });
        
        return Task.CompletedTask;
    }
}
