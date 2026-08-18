using Microsoft.AspNetCore.Http;
using SamorodinkaTech.Fiducia.Domain.Interfaces;

namespace SamorodinkaTech.Fiducia.Infrastructure.Middleware;

/// <summary>
/// Middleware для аудита доступа к страницам сайтов.
/// Фиксирует успешный доступ (PAGE_ACCESS), отказы (PAGE_ACCESS_DENIED) и 404 (PAGE_NOT_FOUND).
/// </summary>
public class PageAccessAuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ISecurityAuditService _auditService;

    private static readonly HashSet<string> ExcludedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".css", ".js", ".woff", ".woff2", ".ttf", ".eot", ".svg", ".ico",
        ".png", ".jpg", ".jpeg", ".gif", ".webp", ".map"
    };

    private static readonly HashSet<string> ExcludedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/_blazor", "/_framework", "/_content", "/css", "/js", "/lib"
    };

    public PageAccessAuditMiddleware(RequestDelegate next, ISecurityAuditService auditService)
    {
        _next = next;
        _auditService = auditService;
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
        var userAgent = context.Request.Headers["User-Agent"].ToString().Replace("\r", " ").Replace("\n", " ");

        await _next(context);

        var statusCode = context.Response.StatusCode;
        var actionCode = GetActionCode(statusCode);
        var description = $"{method} {path} → {statusCode} | UA: {userAgent}";

        await _auditService.LogEventAsync(
            actionCode,
            userIp,
            description,
            userId: userId,
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

    private static string GetActionCode(int statusCode) => statusCode switch
    {
        401 => "PAGE_ACCESS_DENIED",
        403 => "PAGE_ACCESS_DENIED",
        404 => "PAGE_NOT_FOUND",
        _ => "PAGE_ACCESS"
    };

    private static Guid? GetUserId(HttpContext context)
    {
        var userIdClaim = context.User?.FindFirst("sub")?.Value
                       ?? context.User?.FindFirst("user_id")?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
