using System.Security.Claims;
using NariNoteBackend.Domain;

namespace NariNoteBackend.Application.Security;

public interface IJwtHelper
{
    int GetExpirationInHours();
    string GenerateToken(User user, string sessionKey);
    ClaimsPrincipal? ValidateToken(string token);
    string GenerateSessionKey();
}
