namespace NariNoteBackend.Domain.Security;

public interface ICookieOptionsHelper
{
    CookieOptions CreateAuthCookieOptions(TimeSpan maxAge);
}
