using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using SamorodinkaTech.Fiducia.Infrastructure.Common;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;

namespace SamorodinkaTech.Fiducia.BoardPortal;

/// <summary>
/// Хелпер для определения текущего ЮЛ через EcosystemParticipant.
/// JWT-методы делегируются в UserContextHelper (Infrastructure).
/// </summary>
public static class LegalEntityHelper
{
    /// <summary>
    /// Получить ID текущего ЮЛ по login пользователя.
    /// </summary>
    public static async Task<Guid?> GetLegalEntityIdAsync(FiduciaDbContext ctx, string? login)
        => await UserContextHelper.GetLegalEntityIdAsync(ctx, login);

    /// <summary>Перегрузка для endpoints (login из JWT).</summary>
    public static async Task<Guid?> GetLegalEntityIdAsync(FiduciaDbContext ctx, HttpContext http)
        => await UserContextHelper.GetLegalEntityIdAsync(ctx, http);

    /// <summary>Перегрузка для Blazor (login из localStorage).</summary>
    public static async Task<Guid?> GetLegalEntityIdAsync(FiduciaDbContext ctx, IJSRuntime js)
    {
        var login = await GetLoginFromLocalStorageAsync(ctx, js);
        return await UserContextHelper.GetLegalEntityIdAsync(ctx, login);
    }

    /// <summary>Получить login из JWT.</summary>
    public static async Task<string?> GetLoginFromJwtAsync(FiduciaDbContext ctx, HttpContext http)
        => await UserContextHelper.GetLoginFromJwtAsync(ctx, http);

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
        => await UserContextHelper.GetUserInfoAsync(ctx, http);
}
