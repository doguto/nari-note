using System.Security.Claims;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Security;

public interface IJwtHelper
{
    int GetExpiration();
    string GenerateToken(User user);
    ClaimsPrincipal? ValidateToken(string token);
    UserId? GetUserIdFromToken(string token);
    string? GetUserNameFromToken(string token);
    string? GetSecurityStampFromToken(string token);
}
