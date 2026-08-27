using FluentAssertions;

namespace SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

/// <summary>
/// Хелпер для чтения и проверки записей в логе аудита.
/// Логи аудита: BoardPortal/logs/audit/ и AdminConsole/logs/audit/.
/// Формат файла: audit-{yyyyMMddHH}.log
/// Формат строки: [{timestamp}] [AUDIT] {actionCode} | User={login} IP={ip} | {description} | {entityName} {entityId}
/// </summary>
public static class AuditLogHelper
{
    private static readonly string[] AuditLogDirectories =
    [
        "./SamorodinkaTech.Fiducia.BoardPortal/logs/audit",
        "./SamorodinkaTech.Fiducia.AdminConsole/logs/audit"
    ];

    /// <summary>
    /// Найти все директории с логами аудита.
    /// </summary>
    private static string[] FindAuditLogDirectories()
    {
        return AuditLogDirectories.Where(Directory.Exists).ToArray();
    }

    /// <summary>
    /// Получить все файлы лога аудита за текущий час из всех директорий.
    /// </summary>
    private static string[] GetCurrentAuditLogFiles()
    {
        var dirs = FindAuditLogDirectories();
        if (dirs.Length == 0)
            throw new DirectoryNotFoundException(
                $"Директория с логами аудита не найдена. Проверены: {string.Join(", ", AuditLogDirectories)}");

        var fileName = $"audit-{DateTime.UtcNow:yyyyMMddHH}.log";
        var files = dirs
            .Select(d => Path.Combine(d, fileName))
            .Where(File.Exists)
            .ToArray();

        if (files.Length == 0)
            throw new FileNotFoundException(
                $"Файл лога аудита не найден ({fileName}). Проверены: {string.Join(", ", dirs)}");

        return files;
    }

    /// <summary>
    /// Прочитать все строки из файлов лога аудита (все порталы).
    /// </summary>
    public static async Task<IReadOnlyList<string>> ReadAuditLogAsync()
    {
        var files = GetCurrentAuditLogFiles();
        var allLines = new List<string>();
        foreach (var file in files)
        {
            var lines = await File.ReadAllLinesAsync(file);
            allLines.AddRange(lines);
        }
        return allLines;
    }

    /// <summary>
    /// Найти записи аудита, содержащие указанный actionCode.
    /// </summary>
    public static async Task<IReadOnlyList<string>> FindEntriesByActionCodeAsync(string actionCode)
    {
        var lines = await ReadAuditLogAsync();
        return lines
            .Where(l => l.Contains($"[AUDIT] {actionCode}", StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Найти записи аудита для указанного пользователя (логин).
    /// </summary>
    public static async Task<IReadOnlyList<string>> FindEntriesByLoginAsync(string login)
    {
        var lines = await ReadAuditLogAsync();
        return lines
            .Where(l => l.Contains($"User={login}", StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Найти записи аудита, содержащие указанный текст.
    /// </summary>
    public static async Task<IReadOnlyList<string>> FindEntriesContainingAsync(string text)
    {
        var lines = await ReadAuditLogAsync();
        return lines
            .Where(l => l.Contains(text, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Проверить, что вход в систему залогирован для указанного пользователя.
    /// Ожидает запись с actionCode=LOGIN_SUCCESS и login.
    /// </summary>
    public static async Task AssertLoginLoggedAsync(string login)
    {
        var entries = await FindEntriesByActionCodeAsync("LOGIN_SUCCESS");
        var matchingEntries = entries
            .Where(e => e.Contains($"User={login}", StringComparison.OrdinalIgnoreCase))
            .ToList();

        matchingEntries.Should().NotBeEmpty(
            $"Вход пользователя '{login}' должен быть залогирован в аудите (LOGIN_SUCCESS)");
    }

    /// <summary>
    /// Проверить, что выход из системы залогирован для указанного пользователя.
    /// Ожидает запись с actionCode=LOGOUT и login.
    /// </summary>
    public static async Task AssertLogoutLoggedAsync(string login)
    {
        var entries = await FindEntriesByActionCodeAsync("LOGOUT");
        var matchingEntries = entries
            .Where(e => e.Contains($"User={login}", StringComparison.OrdinalIgnoreCase))
            .ToList();

        matchingEntries.Should().NotBeEmpty(
            $"Выход пользователя '{login}' должен быть залогирован в аудите (LOGOUT)");
    }

    /// <summary>
    /// Проверить, что операция создания данных залогирована (DATA:CREATE).
    /// </summary>
    public static async Task AssertDataCreateLoggedAsync(string? descriptionContains = null)
    {
        var entries = await FindEntriesByActionCodeAsync("DATA:CREATE");

        if (descriptionContains is not null)
        {
            entries = entries
                .Where(e => e.Contains(descriptionContains, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        entries.Should().NotBeEmpty(
            $"Операция создания данных должна быть залогирована (DATA:CREATE)" +
            (descriptionContains is not null ? $" с описанием, содержащим '{descriptionContains}'" : ""));
    }

    /// <summary>
    /// Проверить, что операция обновления данных залогирована (DATA:UPDATE).
    /// </summary>
    public static async Task AssertDataUpdateLoggedAsync(string? descriptionContains = null)
    {
        var entries = await FindEntriesByActionCodeAsync("DATA:UPDATE");

        if (descriptionContains is not null)
        {
            entries = entries
                .Where(e => e.Contains(descriptionContains, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        entries.Should().NotBeEmpty(
            $"Операция обновления данных должна быть залогирована (DATA:UPDATE)" +
            (descriptionContains is not null ? $" с описанием, содержащим '{descriptionContains}'" : ""));
    }

    /// <summary>
    /// Проверить, что чтение страницы с идентификатором залогировано.
    /// </summary>
    public static async Task AssertPageAccessLoggedAsync(string path)
    {
        var entries = await FindEntriesContainingAsync(path);
        entries.Should().NotBeEmpty(
            $"Доступ к странице '{path}' должен быть залогирован в аудите");
    }

    /// <summary>
    /// Проверить, что операция с участником залогирована (PARTICIPANT_ACCESS).
    /// </summary>
    public static async Task AssertParticipantAccessLoggedAsync()
    {
        var entries = await FindEntriesByActionCodeAsync("PARTICIPANT_ACCESS");
        entries.Should().NotBeEmpty(
            "Операция с участником должна быть залогирована (PARTICIPANT_ACCESS)");
    }

    /// <summary>
    /// Проверить отсутствие ошибок доступа в логе аудита.
    /// </summary>
    public static async Task AssertNoAccessDeniedAsync()
    {
        var deniedEntries = await FindEntriesByActionCodeAsync("ACCESS:PAGE_DENIED");
        var notFoundEntries = await FindEntriesByActionCodeAsync("ACCESS:PAGE_NOT_FOUND");

        deniedEntries.Should().BeEmpty(
            $"В логе аудита не должно быть записей об отказе в доступе. Найдено: {string.Join("; ", deniedEntries)}");

        notFoundEntries.Should().BeEmpty(
            $"В логе аудита не должно быть записей 'страница не найдена'. Найдено: {string.Join("; ", notFoundEntries)}");
    }

    /// <summary>
    /// Получить количество записей аудита с указанным actionCode.
    /// </summary>
    public static async Task<int> CountEntriesByActionCodeAsync(string actionCode)
    {
        var entries = await FindEntriesByActionCodeAsync(actionCode);
        return entries.Count;
    }
}
