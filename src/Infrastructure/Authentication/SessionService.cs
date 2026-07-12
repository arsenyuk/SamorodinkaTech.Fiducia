using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SamorodinkaTech.Fiducia.Domain.Interfaces;

namespace SamorodinkaTech.Fiducia.Infrastructure.Authentication;

/// <summary>
/// Сервис управления сессиями на основе JWT (УПД.15).
/// </summary>
public class SessionService : ISessionService
{
    private readonly IConfiguration _configuration;
    private readonly int _idleTimeoutMinutes;
    private readonly SigningCredentials _signingCredentials;

    public SessionService(IConfiguration configuration)
    {
        _configuration = configuration;
        _idleTimeoutMinutes = configuration.GetValue<int>("Session:IdleTimeoutMinutes", 5);

        var secretKey = configuration["Session:JwtSecret"]
            ?? throw new InvalidOperationException("Session:JwtSecret is not configured");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        _signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }

    /// <inheritdoc />
    public string GenerateToken(Guid userId, string role)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: "Fiducia",
            audience: "Fiducia",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_idleTimeoutMinutes),
            signingCredentials: _signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <inheritdoc />
    public int? ValidateToken(string token)
    {
        if (string.IsNullOrEmpty(token))
            return null;

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "Fiducia",
            ValidAudience = "Fiducia",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Session:JwtSecret"]!)),
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            var principal = new JwtSecurityTokenHandler().ValidateToken(
                token, validationParameters, out _);

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var _))
                return null; // Keep return type int? for compatibility; we don't use numeric userId elsewhere

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc />
    public int GetIdleTimeoutMinutes() => _idleTimeoutMinutes;
}
