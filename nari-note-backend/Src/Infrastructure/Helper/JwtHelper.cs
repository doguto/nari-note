using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using NariNoteBackend.Domain;

namespace NariNoteBackend.Infrastructure.Helper;

public class JwtHelper
{
    readonly string secret;
    readonly string issuer;
    readonly string audience;
    readonly int expirationInHours;
    
    public JwtHelper(IConfiguration configuration)
    {
        this.secret = configuration["Jwt:Secret"] 
            ?? throw new InvalidOperationException("JWT Secret is not configured");
        this.issuer = configuration["Jwt:Issuer"] 
            ?? throw new InvalidOperationException("JWT Issuer is not configured");
        this.audience = configuration["Jwt:Audience"] 
            ?? throw new InvalidOperationException("JWT Audience is not configured");
        this.expirationInHours = int.Parse(
            configuration["Jwt:ExpirationInHours"] ?? "24");
    }
    
    public int GetExpirationInHours()
    {
        return this.expirationInHours;
    }
    
    public string GenerateToken(User user, string sessionKey)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.Name),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("sessionKey", sessionKey)
        };
        
        var token = new JwtSecurityToken(
            issuer: this.issuer,
            audience: this.audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(this.expirationInHours),
            signingCredentials: credentials
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    public ClaimsPrincipal? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(this.secret);
        
        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = this.issuer,
                ValidateAudience = true,
                ValidAudience = this.audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);
            
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
    
    public static string GenerateSessionKey()
    {
        var randomBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        return Convert.ToBase64String(randomBytes);
    }
}
