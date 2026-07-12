using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SamorodinkaTech.Fiducia.Infrastructure.Middleware;

/// <summary>
/// Логирует все ответы со статусом 404 (Not Found) через ILogger (Serilog).
/// Фиксирует Referer, путь, метод запроса, IP и User-Agent.
/// </summary>
public class NotFoundLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<NotFoundLoggingMiddleware> _logger;

    public NotFoundLoggingMiddleware(
        RequestDelegate next,
        ILogger<NotFoundLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        await _next(context);

        if (context.Response.StatusCode == StatusCodes.Status404NotFound)
        {
            var method = context.Request.Method;
            var path = context.Request.Path.HasValue
                ? context.Request.Path.ToString()
                : "/";
            var query = context.Request.QueryString.HasValue
                ? context.Request.QueryString.Value
                : string.Empty;
            var referer = context.Request.Headers["Referer"].ToString();
            if (string.IsNullOrWhiteSpace(referer)) referer = "(none)";
            referer = referer.Replace("\r", " ").Replace("\n", " ");

            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "(n/a)";
            var ua = context.Request.Headers["User-Agent"].ToString();
            ua = ua.Replace("\r", " ").Replace("\n", " ");

            _logger.LogWarning("404 Not Found: {Method} {Path}{Query} referer={Referer} ip={Ip} ua=\"{UserAgent}\"",
                method, path, query, referer, ip, ua);
        }
    }
}
