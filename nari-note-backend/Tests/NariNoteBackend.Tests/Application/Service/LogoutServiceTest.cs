using Microsoft.AspNetCore.Http;
using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Service;

namespace NariNoteBackend.Tests.Application.Service;

public class LogoutServiceTest
{
    [Fact]
    public async Task 認証Cookieを削除する()
    {
        var httpResponse = new DefaultHttpContext().Response;

        await new LogoutService().ExecuteAsync(new LogoutRequest(), httpResponse);

        var setCookie = httpResponse.Headers.SetCookie.ToString();
        Assert.StartsWith("authToken=;", setCookie);
        Assert.Contains("path=/", setCookie);
        Assert.Contains("expires=Thu, 01 Jan 1970", setCookie);
    }
}
