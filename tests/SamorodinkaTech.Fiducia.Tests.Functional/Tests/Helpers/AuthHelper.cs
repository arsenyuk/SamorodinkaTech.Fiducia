using FluentAssertions;
using Microsoft.Playwright;

namespace SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

/// <summary>
/// Хелпер для авторизации в Admin Console и Board Portal.
/// </summary>
public static class AuthHelper
{
    private const int DefaultTimeout = 15_000;

    /// <summary>
    /// Вход в Admin Console: ввести логин и пароль, кликнуть "Войти".
    /// </summary>
    public static async Task LoginAsAdminAsync(IPage page, string login, string password = "1")
    {
        if (!page.Url.Contains("/login"))
        {
            await page.GotoAsync(PortalUrls.GetUrl(Portal.AdminConsole, "/login"));
            await WaitForBlazorReady(page);
        }
        await page.WaitForTimeoutAsync(2000);

        // Логин
        await page.FillAsync("input[type='text']", login);
        await page.WaitForTimeoutAsync(500);

        // Пароль
        await page.FillAsync("input[type='password']", password);
        await page.WaitForTimeoutAsync(1000);

        // Ждём, пока кнопка станет доступной (Blazor гидрировался и CanLogin=true)
        await page.WaitForFunctionAsync(
            @"() => {
                const btn = document.querySelector('button.btn-primary');
                return btn && !btn.disabled;
            }",
            null,
            new PageWaitForFunctionOptions { Timeout = DefaultTimeout });

        // Клик "Войти"
        await page.ClickAsync("button.btn-primary");

        // Ждём редиректа на /main
        await page.WaitForFunctionAsync(
            "() => window.location.pathname === '/main'",
            null,
            new PageWaitForFunctionOptions { Timeout = DefaultTimeout });
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Вход в Board Portal: ввести логин и пароль, кликнуть "Войти".
    /// </summary>
    public static async Task LoginAsBoardUserAsync(IPage page, string login, string password = "1")
    {
        await page.GotoAsync(PortalUrls.GetUrl(Portal.BoardPortal, "/login"));
        await WaitForBlazorReady(page);
        await page.WaitForTimeoutAsync(2000);

        // Логин
        await page.FillAsync("input[type='text']", login);
        await page.WaitForTimeoutAsync(500);

        // Пароль
        await page.FillAsync("input[type='password']", password);
        await page.WaitForTimeoutAsync(1000);

        // Ждём, пока кнопка станет доступной
        await page.WaitForFunctionAsync(
            @"() => {
                const btn = document.querySelector('button.btn-primary');
                return btn && !btn.disabled;
            }",
            null,
            new PageWaitForFunctionOptions { Timeout = DefaultTimeout });

        // Клик "Войти"
        await page.ClickAsync("button.btn-primary");

        // Ждём редиректа на /main
        await page.WaitForFunctionAsync(
            "() => window.location.pathname === '/main'",
            null,
            new PageWaitForFunctionOptions { Timeout = DefaultTimeout });
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Выход из текущей сессии через API.
    /// </summary>
    public static async Task LogoutAsync(IPage page)
    {
        await page.EvaluateAsync<object>(
            @"async () => {
                await fetch('/api/session/logout', { method: 'POST', credentials: 'same-origin' });
            }");

        // Clear localStorage
        await page.EvaluateAsync("() => { localStorage.clear(); }");
    }

    /// <summary>
    /// Дождаться полной готовности Blazor Server (гидрация + SignalR).
    /// Проверяет наличие и видимость script blazor.server.js.
    /// </summary>
    public static async Task WaitForBlazorReady(IPage page, int timeoutMs = DefaultTimeout)
    {
        await page.WaitForFunctionAsync(
            @"() => !!document.querySelector('script[src*=""blazor.server.js""]')",
            null,
            new PageWaitForFunctionOptions { Timeout = timeoutMs });
    }
}
