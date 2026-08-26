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
    /// Использует поля ввода напрямую, без dropdown.
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
    /// Использует поля ввода напрямую, без dropdown.
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

    private static async Task FillPasswordAsync(IPage page, string password)
    {
        // PasswordInput использует два input: visible text + hidden password (opacity:0)
        // Заполняем через JavaScript, чтобы обойти opacity:hidden
        await page.EvaluateAsync(
            $@"() => {{
                const inputs = document.querySelectorAll('input[type=""password""]');
                for (const input of inputs) {{
                    if (input.style.opacity === '0' || input.closest('.password-input')) {{
                        const nativeInputValueSetter = Object.getOwnPropertyDescriptor(window.HTMLInputElement.prototype, 'value').set;
                        nativeInputValueSetter.call(input, '{EscapeJs(password)}');
                        input.dispatchEvent(new Event('input', {{ bubbles: true }}));
                        input.dispatchEvent(new Event('change', {{ bubbles: true }}));
                        return;
                    }}
                }}
            }}");
    }

    private static async Task ClickLoginButtonAsync(IPage page)
    {
        // Принудительно снимаем disabled и кликаем
        await page.EvaluateAsync(
            @"() => {
                const btn = document.querySelector('button.btn-primary');
                if (btn) {
                    btn.disabled = false;
                    btn.click();
                }
            }");

        // Wait for loading spinner to disappear (if any)
        await page.WaitForFunctionAsync(
            @"() => document.querySelector('.spinner-border') === null ||
                     document.querySelector('.spinner-border')?.offsetParent === null",
            null,
            new PageWaitForFunctionOptions { Timeout = DefaultTimeout });
    }

    /// <summary>
    /// Дождаться полной готовности Blazor Server (гидрация + SignalR).
    /// </summary>
    public static async Task WaitForBlazorReady(IPage page, int timeoutMs = DefaultTimeout)
    {
        // Ждём загрузки blazor.server.js
        await page.WaitForFunctionAsync(
            @"() => !!document.querySelector('script[src*=""blazor.server.js""]')",
            null,
            new PageWaitForFunctionOptions { Timeout = timeoutMs });
    }

    private static string EscapeJs(string value) => value.Replace("'", "\\'").Replace("\\", "\\\\");
}
