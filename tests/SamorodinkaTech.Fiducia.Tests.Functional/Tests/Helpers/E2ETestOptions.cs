namespace SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

/// <summary>
/// Конфигурация E2E-тестов (из appsettings.test.json).
/// </summary>
public sealed class E2ETestOptions
{
    /// <summary>Таймаут ожидания элементов (мс) для Playwright WaitForSelector/WaitForFunction.</summary>
    public int TimeoutMs { get; init; } = 5000;
}
