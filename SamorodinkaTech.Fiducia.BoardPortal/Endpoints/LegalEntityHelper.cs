using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;

namespace SamorodinkaTech.Fiducia.BoardPortal;

/// <summary>
/// Хелпер для определения текущего ЮЛ через EcosystemParticipant (вместо current_workplace).
/// </summary>
public static class LegalEntityHelper
{
    /// <summary>
    /// Получить ID текущего ЮЛ по login пользователя из JWT.
    /// </summary>
    public static async Task<Guid?> GetLegalEntityIdAsync(FiduciaDbContext ctx, HttpContext http)
    {
        var userIdStr = http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return null;

        var user = await ctx.Users.FindAsync(userId);
        if (user is null) return null;

        var participant = await ctx.EcosystemParticipants
            .FirstOrDefaultAsync(ep => ep.Login == user.Login);
        return participant?.LegalEntityId;
    }

    /// <summary>
    /// Получить login текущего пользователя из JWT.
    /// </summary>
    public static async Task<(string? login, string fullName)> GetUserInfoAsync(FiduciaDbContext ctx, HttpContext http)
    {
        var userIdStr = http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return (null, "Неизвестный пользователь");

        var user = await ctx.Users.FindAsync(userId);
        if (user is null)
            return (null, "Пользователь не найден");

        var login = user.Login;
        var fullName = string.IsNullOrWhiteSpace(user.MiddleName)
            ? $"{user.LastName} {user.FirstName}"
            : $"{user.LastName} {user.FirstName} {user.MiddleName}";

        return (login, fullName);
    }
}
