using Microsoft.Playwright;
using Microsoft.Extensions.Configuration;
using SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

namespace SamorodinkaTech.Fiducia.Tests.Functional;

/// <summary>
/// Assembly-level fixture — глобальный setup перед ВСЕМИ E2E-тестами.
/// Выполняется ОДИН раз для всей сборки.
/// Включает инициализацию инфраструктуры и создание браузера.
/// </summary>
public class GlobalFixture : IAsyncLifetime
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private static volatile bool _hasFailed;

    /// <summary>Публичный доступ к Playwright для тестов.</summary>
    public IPlaywright Playwright => _playwright;

    /// <summary>Публичный доступ к браузеру для тестов.</summary>
    public IBrowser Browser => _browser;

    /// <summary>Флаг: хотя бы один тест завершился с ошибкой.</summary>
    public static bool HasFailed => _hasFailed;

    /// <summary>Установить флаг ошибки.</summary>
    public static void MarkFailed() => _hasFailed = true;

    /// <summary>Конфигурация E2E-тестов (таймауты и т.п.).</summary>
    public static E2ETestOptions TestOptions { get; private set; } = new();

    public ValueTask InitializeAsync()
    {
        return new ValueTask(Task.Run(async () =>
        {
            Console.WriteLine("[GlobalFixture] Инициализация...");

            // 0. Загрузка конфигурации
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json", optional: true)
                .Build();
            var timeoutStr = config["E2ETest:TimeoutMs"];
            var timeoutMs = int.TryParse(timeoutStr, out var t) ? t : 5000;
            TestOptions = new E2ETestOptions { TimeoutMs = timeoutMs };
            Console.WriteLine($"[GlobalFixture] TimeoutMs = {TestOptions.TimeoutMs}");

            // 1. Запуск инфраструктуры (Docker, порталы)
            await InfrastructureHelper.EnsureInfrastructureReadyAsync();

            // 2. Создание Playwright и браузера
            _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new() { Headless = false });

            // 3. Сброс БД + пересоздание LDAP-пользователей (один раз, через CLI — без браузера)
            await CharterTestGlobalInit.InitializeAsync();

            Console.WriteLine("[GlobalFixture] Инициализация завершена.");
        }));
    }

    public ValueTask DisposeAsync()
    {
        return new ValueTask(Task.Run(async () =>
        {
            if (_browser is not null)
                await _browser.CloseAsync();
            _playwright?.Dispose();
        }));
    }
}
