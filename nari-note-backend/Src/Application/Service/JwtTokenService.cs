using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NariNoteBackend.Domain;

namespace NariNoteBackend.Application.Service;

public class JwtTokenService
{
    readonly IConfiguration configuration;
    
    public JwtTokenService(IConfiguration configuration)
    {
        this.configuration = configuration;
    }
    
    /// <summary>
    /// JWTトークンを生成する
    /// </summary>
    public string GenerateToken(User user)
    {
        var secret = configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
        var issuer = configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
        var audience = configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");
        var expirationHours = int.Parse(configuration["Jwt:ExpirationInHours"] ?? "24");
        
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.Name),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expirationHours),
            signingCredentials: credentials
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    /// <summary>
    /// セッションキーを生成する
    /// </summary>
    public string GenerateSessionKey()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
