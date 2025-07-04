using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Zentry.Modules.UserManagement.Services;

public class JwtService : IJwtService
{
    private readonly string _audience;
    private readonly IConfiguration _configuration;
    private readonly int _expirationMinutes;
    private readonly string _issuer;
    private readonly string _secret;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
        _secret = _configuration["Jwt:Secret"] ?? throw new ArgumentNullException("JWT Secret not configured.");
        _issuer = _configuration["Jwt:Issuer"] ?? "Zentry"; // Default issuer
        _audience = _configuration["Jwt:Audience"] ?? "ZentryUsers"; // Default audience
        _expirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60"); // Default 60 minutes
    }

    public string GenerateToken(Guid userId, string email, string role)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secret);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_expirationMinutes),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}