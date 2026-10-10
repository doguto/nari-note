using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using NariNoteBackend.Domain.Security;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Extension;

namespace NariNoteBackend.Infrastructure.Security;

public class JwtHelper : IJwtHelper
{
    readonly string audience;
    readonly int expirationInHours;
    readonly string issuer;
    readonly string secret;
    readonly TimeProvider timeProvider;

    public JwtHelper(IConfiguration configuration, TimeProvider timeProvider)
    {
        this.timeProvider = timeProvider;

        secret = configuration["Jwt:Secret"]
                 ?? throw new InvalidOperationException("JWT Secret is not configured");
        issuer = configuration["Jwt:Issuer"]
                 ?? throw new InvalidOperationException("JWT Issuer is not configured");
        audience = configuration["Jwt:Audience"]
                   ?? throw new InvalidOperationException("JWT Audience is not configured");
        expirationInHours = int.Parse(configuration["Jwt:ExpirationInHours"] ?? "24");
    }

    SymmetricSecurityKey SymmetricSecurityKey => new(Encoding.UTF8.GetBytes(secret));

    public int GetExpiration()
    {
        return expirationInHours;
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        // ClaimタイプをASP.NETが自動でマッピングしてしまうのを無効化
        tokenHandler.InboundClaimTypeMap.Clear();

        try
        {
            var principal = tokenHandler.ValidateToken(
                token,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = SymmetricSecurityKey,
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    // 発行時と同じ TimeProvider で有効期限を判定する
                    LifetimeValidator = IsWithinLifetime,
                    ClockSkew = TimeSpan.Zero
                },
                out _
            );

            return principal;
        }
        catch (SecurityTokenException)
        {
            return null;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    public UserId? GetUserIdFromToken(string token)
    {
        var principal = ValidateToken(token);
        if (principal == null) return null;

        var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub);
        if (userIdClaim == null) return null;

        if (Guid.TryParse(userIdClaim.Value, out var userIdValue)) return UserId.From(userIdValue);

        return null;
    }

    public string GenerateToken(UserId userId, string userName)
    {
        var credentials = new SigningCredentials(SymmetricSecurityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.Value.ToString()),
            new Claim(JwtRegisteredClaimNames.Name, userName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: timeProvider.UtcNow().AddHours(expirationInHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string? GetUserNameFromToken(string token)
    {
        var principal = ValidateToken(token);
        if (principal == null) return null;

        var userNameClaim = principal.FindFirst(JwtRegisteredClaimNames.Name);
        if (userNameClaim == null) return null;

        return userNameClaim.Value;
    }

    bool IsWithinLifetime(
        DateTime? notBefore,
        DateTime? expires,
        SecurityToken securityToken,
        TokenValidationParameters validationParameters
    )
    {
        var now = timeProvider.UtcNow();
        if (!expires.HasValue) return false;
        if (notBefore.HasValue && notBefore.Value > now) return false;

        return expires.Value >= now;
    }
}
