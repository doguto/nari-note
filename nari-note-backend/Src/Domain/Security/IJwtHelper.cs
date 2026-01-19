using System.Security.Claims;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Security;

public interface IJwtHelper
{
    int GetExpirationInHours();
    string GenerateToken(UserId userId);
    ClaimsPrincipal? ValidateToken(string token);
    UserId? GetUserIdFromToken(string token);
}
