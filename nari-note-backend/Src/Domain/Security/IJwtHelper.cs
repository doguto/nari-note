using System.Security.Claims;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Security;

public interface IJwtHelper
{
    int GetExpirationInHours();
    string GenerateToken(User user);
    ClaimsPrincipal? ValidateToken(string token);
    UserId? GetUserIdFromToken(string token);
}
