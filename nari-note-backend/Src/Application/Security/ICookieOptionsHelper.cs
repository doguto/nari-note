using Microsoft.AspNetCore.Http;

namespace NariNoteBackend.Application.Security;

public interface ICookieOptionsHelper
{
    CookieOptions CreateAuthCookieOptions(TimeSpan maxAge);
}
