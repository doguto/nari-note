using System.Security.Claims;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Security;

public interface IJwtHelper
{
    int GetExpirationInHours();
    string GenerateToken(UserId userId, string userName);
    ClaimsPrincipal? ValidateToken(string token);
    UserId? GetUserIdFromToken(string token);
    string? GetUserNameFromToken(string token);
}
