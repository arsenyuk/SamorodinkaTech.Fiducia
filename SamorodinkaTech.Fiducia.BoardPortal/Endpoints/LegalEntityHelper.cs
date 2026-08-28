using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using System.Security.Claims;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;

namespace SamorodinkaTech.Fiducia.BoardPortal;

/// <summary>
/// Хелпер для определения текущего ЮЛ через EcosystemParticipant (вместо current_workplace).
/// </summary>
public static class LegalEntityHelper
{
    /// <summary>
    /// Получить ID текущего ЮЛ по login пользователя.
    /// </summary>
    public static async Task<Guid?> GetLegalEntityIdAsync(FiduciaDbContext ctx, string? login)
    {
        if (string.IsNullOrEmpty(login)) return null;

        var participant = await ctx.EcosystemParticipants
            .FirstOrDefaultAsync(ep => ep.Login == login);
        return participant?.LegalEntityId;
    }

    /// <summary>Перегрузка для endpoints (login из JWT).</summary>
    public static async Task<Guid?> GetLegalEntityIdAsync(FiduciaDbContext ctx, HttpContext http)
    {
        var login = await GetLoginFromJwtAsync(ctx, http);
        return await GetLegalEntityIdAsync(ctx, login);
    }

    /// <summary>Перегрузка для Blazor (login из localStorage).</summary>
    public static async Task<Guid?> GetLegalEntityIdAsync(FiduciaDbContext ctx, IJSRuntime js)
    {
        var login = await GetLoginFromLocalStorageAsync(ctx, js);
        return await GetLegalEntityIdAsync(ctx, login);
    }

    /// <summary>Получить login из JWT.</summary>
    public static async Task<string?> GetLoginFromJwtAsync(FiduciaDbContext ctx, HttpContext http)
    {
        var userIdStr = http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return null;

        var user = await ctx.Users.FindAsync(userId);
        return user?.Login;
    }

    /// <summary>Получить login из localStorage.</summary>
    public static async Task<string?> GetLoginFromLocalStorageAsync(FiduciaDbContext ctx, IJSRuntime js)
    {
        var userIdStr = await js.InvokeAsync<string?>("localStorage.getItem", "currentUserId");
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            return null;

        var user = await ctx.Users.FindAsync(userId);
        return user?.Login;
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
}
