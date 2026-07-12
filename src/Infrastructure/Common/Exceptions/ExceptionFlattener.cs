using System.Collections;
using Microsoft.Extensions.Logging;

namespace SamorodinkaTech.Fiducia.Infrastructure.Common.Exceptions;

public static class ExceptionFlattener
{
    public record ExceptionInfo(string Type, string Message, string? StackTrace);

    public static List<ExceptionInfo> Flatten(Exception ex)
    {
        var result = new List<ExceptionInfo>();
        Traverse(ex, result);
        return result;
    }

    private static void Traverse(Exception ex, List<ExceptionInfo> acc)
    {
        if (ex == null) return;
        acc.Add(new ExceptionInfo(ex.GetType().FullName ?? ex.GetType().Name, ex.Message, ex.StackTrace));

        // Если это AggregateException — пройтись по InnerExceptions
        if (ex is AggregateException aex && aex.InnerExceptions is { Count: > 0 })
        {
            foreach (var ie in aex.InnerExceptions)
                Traverse(ie, acc);
            return;
        }

        // Некоторые исключения (например, ReflectionTypeLoadException) хранят коллекцию во вложенном свойстве
        var innerExceptionsProp = ex.GetType().GetProperty("InnerExceptions");
        if (innerExceptionsProp?.GetValue(ex) is IEnumerable enumerable)
        {
            foreach (var ie in enumerable)
                if (ie is Exception e) Traverse(e, acc);
            return;
        }

        if (ex.InnerException != null)
            Traverse(ex.InnerException, acc);
    }

    public static void LogFlattened(ILogger logger, Exception ex)
    {
        var infos = Flatten(ex);
        logger.LogError("Unhandled exception(s): {Count}", infos.Count);
        foreach (var i in infos)
        {
            logger.LogError("{Type}: {Message}\n{Stack}", i.Type, i.Message, i.StackTrace);
        }
    }
}
