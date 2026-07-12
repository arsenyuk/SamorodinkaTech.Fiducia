using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Infrastructure.Common.Exceptions;

namespace SamorodinkaTech.Fiducia.Infrastructure.Middleware;

/// <summary>
/// Middleware для централизованного логирования необработанных исключений.
/// Пишет через ILogger (Serilog) и пробрасывает исключение дальше.
/// </summary>
public class ExceptionLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionLoggingMiddleware> _logger;

    public ExceptionLoggingMiddleware(RequestDelegate next,
        ILogger<ExceptionLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            ExceptionFlattener.LogFlattened(_logger, ex);
            throw;
        }
    }
}
