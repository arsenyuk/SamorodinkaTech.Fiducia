using System.Runtime.CompilerServices;
using Serilog;

namespace SamorodinkaTech.Fiducia.Tests.Functional;

/// <summary>
/// Инициализирует Serilog для функциональных тестов.
/// Логи пишутся в ./logs/tests/functional/functional-test-.log
/// </summary>
public static class TestLogging
{
    [ModuleInitializer]
    public static void Initialize()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} | {Message:lj}{NewLine}{Exception}")
            .WriteTo.File("./logs/tests/functional/functional-test-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} | {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }
}
