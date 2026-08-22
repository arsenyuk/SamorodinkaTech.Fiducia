using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Infrastructure.Persistence;

namespace SamorodinkaTech.Fiducia.Infrastructure.Middleware;

/// <summary>
/// Middleware для аудита доступа к страницам сайтов.
/// Фиксирует только бизнес-события: переходы между страницами, ошибки доступа, 404.
/// Служебные API-вызовы (Blazor-роутинг, справочники) исключены из аудита.
/// </summary>
public class PageAccessAuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ISecurityAuditService _auditService;
    private readonly IDbContextFactory<FiduciaDbContext> _dbFactory;

    private static readonly HashSet<string> ExcludedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".css", ".js", ".woff", ".woff2", ".ttf", ".eot", ".svg", ".ico",
        ".png", ".jpg", ".jpeg", ".gif", ".webp", ".map"
    };

    /// <summary>
    /// Пути, не попадающие в лог аудита (служебные, технические).
    /// </summary>
    private static readonly HashSet<string> ExcludedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/_blazor", "/_framework", "/_content", "/css", "/js", "/lib"
    };

    /// <summary>
    /// API-пути, не попадающие в лог аудита (служебные Blazor-вызовы, справочники).
    /// </summary>
    private static readonly HashSet<string> ExcludedApiPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/api/session/config", "/api/session/login", "/api/session/logout",
        "/api/share-requests/types"
    };

    public PageAccessAuditMiddleware(RequestDelegate next, ISecurityAuditService auditService, IDbContextFactory<FiduciaDbContext> dbFactory)
    {
        _next = next;
        _auditService = auditService;
        _dbFactory = dbFactory;
    }

    public async Task Invoke(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "/";

        if (IsExcludedPath(path))
        {
            await _next(context);
            return;
        }

        var userId = GetUserId(context);
        var userIp = ClientIpHelper.GetClientIp(context);
        var method = context.Request.Method;

        // HEAD-запросы — health-check, не логируем
        if (string.Equals(method, "HEAD", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        await _next(context);

        var statusCode = context.Response.StatusCode;

        // Пропускаем успешные API-вызовы (стандартные Blazor/SPA паттерны)
        if (IsExcludedApiPath(path) && statusCode == 200)
            return;

        // Логируем ТОЛЬКО: модификации данных (POST/PUT/DELETE) и ошибки доступа (4xx/5xx)
        var isModification = string.Equals(method, "POST", StringComparison.OrdinalIgnoreCase)
            || string.Equals(method, "PUT", StringComparison.OrdinalIgnoreCase)
            || string.Equals(method, "PATCH", StringComparison.OrdinalIgnoreCase)
            || string.Equals(method, "DELETE", StringComparison.OrdinalIgnoreCase);
        var isError = statusCode >= 400;

        if (!isModification && !isError)
            return;

        var actionCode = GetActionCode(method, statusCode, path);
        var description = GetDescription(method, path, statusCode);

        // Загружаем логин пользователя из БД
        string? login = null;
        if (userId.HasValue)
        {
            try
            {
                await using var ctx = await _dbFactory.CreateDbContextAsync();
                var user = await ctx.Users
                    .Where(u => u.Id == userId.Value)
                    .Select(u => u.Login)
                    .FirstOrDefaultAsync();
                login = user;
            }
            catch { /* не критично */ }
        }

        await _auditService.LogEventAsync(
            actionCode,
            userIp,
            description,
            userId: userId,
            login: login,
            entityName: "Page",
            entityId: null);
    }

    private static bool IsExcludedPath(string path)
    {
        if (ExcludedPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            return true;

        var extension = Path.GetExtension(path);
        return !string.IsNullOrEmpty(extension) && ExcludedExtensions.Contains(extension);
    }

    private static bool IsExcludedApiPath(string path)
    {
        return ExcludedApiPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));
    }

    private static string GetActionCode(string method, int statusCode, string path) => statusCode switch
    {
        401 => "ACCESS:PAGE_DENIED",
        403 => "ACCESS:PAGE_DENIED",
        404 => "ACCESS:PAGE_NOT_FOUND",
        _ when method == "POST" => "DATA:CREATE",
        _ when method == "PUT" || method == "PATCH" => "DATA:UPDATE",
        _ when method == "DELETE" => "DATA:DELETE",
        _ => "DATA:READ"
    };

    private static string GetDescription(string method, string path, int statusCode)
    {
        var statusText = statusCode switch
        {
            200 => "успешно",
            401 => "отказ (не аутентифицирован)",
            403 => "отказ (нет доступа)",
            404 => "не найдено",
            400 => "ошибка запроса",
            500 => "ошибка сервера",
            _ => $"код {statusCode}"
        };

        return $"{method} {path} — {statusText}";
    }

    private static Guid? GetUserId(HttpContext context)
    {
        var userIdClaim = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? context.User?.FindFirst("sub")?.Value
                       ?? context.User?.FindFirst("user_id")?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
