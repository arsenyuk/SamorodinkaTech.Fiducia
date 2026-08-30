using Microsoft.Playwright;
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

    /// <summary>Публичный доступ к Playwright для тестов.</summary>
    public IPlaywright Playwright => _playwright;

    /// <summary>Публичный доступ к браузеру для тестов.</summary>
    public IBrowser Browser => _browser;

    public ValueTask InitializeAsync()
    {
        return new ValueTask(Task.Run(async () =>
        {
            Console.WriteLine("[GlobalFixture] Инициализация...");

            // 1. Запуск инфраструктуры (Docker, порталы)
            await InfrastructureHelper.EnsureInfrastructureReadyAsync();

            // 2. Создание Playwright и браузера
            _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new() { Headless = true });

            // 3. Сброс БД + пересоздание LDAP-пользователей (один раз)
            var adminPage = await _browser.NewPageAsync(new() { IgnoreHTTPSErrors = true });
            var ldapPage = await _browser.NewPageAsync(new() { IgnoreHTTPSErrors = true });

            try
            {
                await CharterTestGlobalInit.InitializeAsync(adminPage, ldapPage);
            }
            finally
            {
                await adminPage.CloseAsync();
                await ldapPage.CloseAsync();
            }

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
