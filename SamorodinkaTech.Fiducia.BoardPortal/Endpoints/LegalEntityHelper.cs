using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SamorodinkaTech.Fiducia.Infrastructure.Common;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;

namespace SamorodinkaTech.Fiducia.BoardPortal;

/// <summary>
/// Хелпер для определения текущего ЮЛ через EcosystemParticipant.
/// JWT-методы делегируются в UserContextHelper (Infrastructure).
/// </summary>
public static class LegalEntityHelper
{
    /// <summary>Получить userId из JWT ( делегирует в UserContextHelper ).</summary>
    public static Guid? GetUserIdAsync(HttpContext http)
        => UserContextHelper.GetUserIdAsync(http);

    /// <summary>
    /// Получить ID текущего ЮЛ по login пользователя.
    /// </summary>
    public static async Task<Guid?> GetLegalEntityIdAsync(FiduciaDbContext ctx, string? login)
        => await UserContextHelper.GetLegalEntityIdAsync(ctx, login);

    /// <summary>Перегрузка для endpoints (login из JWT).</summary>
    public static async Task<Guid?> GetLegalEntityIdAsync(FiduciaDbContext ctx, HttpContext http)
        => await UserContextHelper.GetLegalEntityIdAsync(ctx, http);

    /// <summary>Перегрузка для Blazor (login из JWT через HttpContextAccessor).</summary>
    public static async Task<Guid?> GetLegalEntityIdAsync(FiduciaDbContext ctx, IHttpContextAccessor httpCtxAccessor)
        => await UserContextHelper.GetLegalEntityIdAsync(ctx, httpCtxAccessor.HttpContext!);

    /// <summary>Получить login из JWT.</summary>
    public static async Task<string?> GetLoginFromJwtAsync(FiduciaDbContext ctx, HttpContext http)
        => await UserContextHelper.GetLoginFromJwtAsync(ctx, http);

    /// <summary>Получить login и ФИО из JWT.</summary>
    public static async Task<(string? login, string fullName)> GetUserInfoAsync(FiduciaDbContext ctx, HttpContext http)
        => await UserContextHelper.GetUserInfoAsync(ctx, http);
}
