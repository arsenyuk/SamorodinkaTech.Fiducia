using FluentAssertions;

namespace SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

/// <summary>
/// Хелпер для чтения и проверки записей в логе аудита.
/// Логи аудита хранятся в ./logs/audit/ (Dev) или /var/log/fiducia/audit/ (Prod).
/// Формат файла: audit-{yyyyMMddHH}.log
/// Формат строки: [{timestamp}] [AUDIT] {actionCode} | User={login} IP={ip} | {description} | {entityName} {entityId}
/// </summary>
public static class AuditLogHelper
{
    private static readonly string[] AuditLogDirectories =
    [
        "./logs/audit",
        "/var/log/fiducia/audit"
    ];

    /// <summary>
    /// Найти директорию с логами аудита.
    /// </summary>
    private static string FindAuditLogDirectory()
    {
        foreach (var dir in AuditLogDirectories)
        {
            if (Directory.Exists(dir))
                return dir;
        }

        throw new DirectoryNotFoundException(
            $"Директория с логами аудита не найдена. Проверены: {string.Join(", ", AuditLogDirectories)}");
    }

    /// <summary>
    /// Получить файл лога аудита за текущий час.
    /// </summary>
    private static string GetCurrentAuditLogFile()
    {
        var dir = FindAuditLogDirectory();
        var fileName = $"audit-{DateTime.UtcNow:yyyyMMddHH}.log";
        var filePath = Path.Combine(dir, fileName);

        if (!File.Exists(filePath))
            throw new FileNotFoundException(
                $"Файл лога аудита не найден: {filePath}. Убедитесь, что приложение запущено и генерирует аудит-события.");

        return filePath;
    }

    /// <summary>
    /// Прочитать все строки из текущего файла лога аудита.
    /// </summary>
    public static async Task<IReadOnlyList<string>> ReadAuditLogAsync()
    {
        var filePath = GetCurrentAuditLogFile();
        var lines = await File.ReadAllLinesAsync(filePath);
        return lines;
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
