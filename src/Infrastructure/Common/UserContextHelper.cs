using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;

namespace SamorodinkaTech.Fiducia.Infrastructure.Common;

/// <summary>
/// Общий хелпер для определения текущего пользователя и ЮЛ через JWT claims.
/// Используется и Admin Console, и Board Portal.
/// </summary>
public static class UserContextHelper
{
    /// <summary>Получить userId из JWT (ClaimTypes.NameIdentifier → Guid).</summary>
    public static Guid? GetUserIdAsync(HttpContext http)
    {
        var userIdStr = http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdStr, out var userId) ? userId : null;
    }

    /// <summary>Получить login из JWT ( ClaimTypes.NameIdentifier → Users.Login ).</summary>
    public static async Task<string?> GetLoginFromJwtAsync(FiduciaDbContext ctx, HttpContext http)
    {
        var userId = GetUserIdAsync(http);
        if (userId is null) return null;

        var user = await ctx.Users.FindAsync(userId.Value);
        return user?.Login;
    }

    /// <summary>Получить ID текущего ЮЛ по JWT.</summary>
    public static async Task<Guid?> GetLegalEntityIdAsync(FiduciaDbContext ctx, HttpContext http)
    {
        var userId = GetUserIdAsync(http);
        if (userId is null) return null;

        var ep = await ctx.EcosystemParticipants
            .FirstOrDefaultAsync(ep => ep.UserId == userId.Value);
        return ep?.LegalEntityId;
    }

    /// <summary>Получить ID текущего ЮЛ по login.</summary>
    public static async Task<Guid?> GetLegalEntityIdAsync(FiduciaDbContext ctx, string? login)
    {
        if (string.IsNullOrEmpty(login)) return null;

        var participant = await ctx.EcosystemParticipants
            .Include(x => x.User)
            .FirstOrDefaultAsync(ep => ep.User != null && ep.User.Login == login);
        return participant?.LegalEntityId;
    }

    /// <summary>Получить login и ФИО из JWT.</summary>
    public static async Task<(string? login, string fullName)> GetUserInfoAsync(FiduciaDbContext ctx, HttpContext http)
    {
        var login = await GetLoginFromJwtAsync(ctx, http);
        if (login is null) return (null, "Неизвестный пользователь");

        var user = await ctx.Users.FirstOrDefaultAsync(u => u.Login == login);
        if (user is null) return (null, "Пользователь не найден");

        var fullName = string.IsNullOrWhiteSpace(user.MiddleName)
            ? $"{user.LastName} {user.FirstName}"
            : $"{user.LastName} {user.FirstName} {user.MiddleName}";

        return (login, fullName);
    }

    /// <summary>Получить ФИО пользователя по userId из БД.</summary>
    public static async Task<string> GetFullNameAsync(FiduciaDbContext ctx, Guid userId)
    {
        var user = await ctx.Users.FindAsync(userId);
        if (user is null) return "Гость";
        return string.IsNullOrWhiteSpace(user.MiddleName)
            ? $"{user.LastName} {user.FirstName}"
            : $"{user.LastName} {user.FirstName} {user.MiddleName}";
    }
}
