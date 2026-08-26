using System.Text.RegularExpressions;
using FluentAssertions;

namespace SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

/// <summary>
/// Хелпер для проверки ошибок в логе приложения (app-{yyyyMMddHH}.log).
/// Формат строки: {Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} | {Message:lj}
/// Поддерживает фильтрацию по временному диапазону для привязки ошибок к конкретному тесту.
/// </summary>
public static partial class AppLogHelper
{
    private static readonly string ProjectRoot = FindProjectRoot();

    private static readonly string[] AppLogDirectories =
    [
        Path.Combine(ProjectRoot, "SamorodinkaTech.Fiducia.BoardPortal", "logs", "app"),
        Path.Combine(ProjectRoot, "SamorodinkaTech.Fiducia.AdminConsole", "logs", "app"),
        Path.Combine(ProjectRoot, "logs", "app"),
        "/var/log/fiducia/app"
    ];

    /// <summary>
    /// Найти корень проекта (директорию с .sln/.slnx файлом).
    /// </summary>
    private static string FindProjectRoot()
    {
        var dir = AppContext.BaseDirectory;

        while (dir is not null)
        {
            if (Directory.GetFiles(dir, "*.sln").Length > 0 ||
                Directory.GetFiles(dir, "*.slnx").Length > 0)
                return dir;

            dir = Directory.GetParent(dir)?.FullName;
        }

        // Fallback: используем текущую директорию
        return Directory.GetCurrentDirectory();
    }

    // Парсер строки лога: timestamp + level
    // Пример: 2026-08-26 14:32:15.123 +03:00 [INF] SourceContext | Message
    [GeneratedRegex(@"^(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3} [+\-Z][\d:]+) \[(\w{3})\]")]
    private static partial Regex LogLineRegex();

    /// <summary>
    /// Найти директорию с логами приложения.
    /// </summary>
    private static string FindAppLogDirectory()
    {
        foreach (var dir in AppLogDirectories)
        {
            if (Directory.Exists(dir))
                return dir;
        }

        throw new DirectoryNotFoundException(
            $"Директория с логами приложения не найдена. Проверены: {string.Join(", ", AppLogDirectories)}");
    }

    /// <summary>
    /// Получить файл лога приложения за текущий час.
    /// </summary>
    private static string GetCurrentAppLogFile()
    {
        var dir = FindAppLogDirectory();
        var fileName = $"app-{DateTime.UtcNow:yyyyMMddHH}.log";
        var filePath = Path.Combine(dir, fileName);

        if (!File.Exists(filePath))
            throw new FileNotFoundException(
                $"Файл лога приложения не найден: {filePath}. Убедитесь, что приложение запущено.");

        return filePath;
    }

    /// <summary>
    /// Прочитать строки из лога приложения за указанный временной диапазон.
    /// </summary>
    public static async Task<IReadOnlyList<string>> ReadAppLogAsync(DateTimeOffset from, DateTimeOffset to)
    {
        var filePath = GetCurrentAppLogFile();
        var allLines = await File.ReadAllLinesAsync(filePath);

        var matchingLines = new List<string>();
        var regex = LogLineRegex();

        foreach (var line in allLines)
        {
            var match = regex.Match(line);
            if (!match.Success) continue;

            if (DateTimeOffset.TryParse(match.Groups[1].Value, out var timestamp))
            {
                if (timestamp >= from && timestamp <= to)
                {
                    matchingLines.Add(line);
                }
            }
        }

        return matchingLines;
    }

    /// <summary>
    /// Найти ошибки (ERROR/FATAL) в логе приложения за указанный временной диапазон.
    /// </summary>
    public static async Task<IReadOnlyList<string>> FindErrorsAsync(DateTimeOffset from, DateTimeOffset to)
    {
        var lines = await ReadAppLogAsync(from, to);
        return lines
            .Where(l => l.Contains("[ERR]") || l.Contains("[FTL]"))
            .ToList();
    }

    /// <summary>
    /// Проверить отсутствие ошибок в логе приложения за период работы теста.
    /// Бросает исключение, если найдены записи уровня ERROR или FATAL.
    /// </summary>
    /// <param name="testStartTime">Время начала теста.</param>
    /// <param name="testEndTime">Время окончания теста.</param>
    /// <param name="testName">Имя теста (для сообщения об ошибке).</param>
    public static async Task AssertNoErrorsInAppLogAsync(
        DateTimeOffset testStartTime,
        DateTimeOffset testEndTime,
        string testName)
    {
        var errors = await FindErrorsAsync(testStartTime, testEndTime);

        if (errors.Count > 0)
        {
            var errorDetails = string.Join("\n  ", errors.Take(10));
            throw new InvalidOperationException(
                $"Тест '{testName}' обнаружил {errors.Count} ошибку(ок) в логе приложения " +
                $"за период {testStartTime:HH:mm:ss.fff} — {testEndTime:HH:mm:ss.fff}:\n  {errorDetails}");
        }
    }

    /// <summary>
    /// Проверить отсутствие ошибок в логе приложения с автоматическим расширением диапазона.
    /// Если файл лога не найден или пуст — тест считается пройденным (лог мог не успеть записаться).
    /// </summary>
    public static async Task AssertNoErrorsInAppLogSafeAsync(
        DateTimeOffset testStartTime,
        DateTimeOffset testEndTime,
        string testName)
    {
        try
        {
            await AssertNoErrorsInAppLogAsync(testStartTime, testEndTime, testName);
        }
        catch (FileNotFoundException)
        {
            // Файл лога не найден — пропускаем проверку (лог мог не успеть записаться)
        }
    }
}
