using Microsoft.Playwright;

namespace SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

/// <summary>
/// Хелпер для авторизации в Admin Console и Board Portal.
/// </summary>
public static class AuthHelper
{
    private const int DefaultTimeout = 15_000;

    /// <summary>
    /// Вход в Admin Console (Basic auth): выбрать пользователя из выпадающего списка, ввести пароль, кликнуть "Войти".
    /// </summary>
    public static async Task LoginAsAdminAsync(IPage page, string userDisplayName, string password = "1")
    {
        await page.GotoAsync(PortalUrls.GetUrl(Portal.AdminConsole, "/login"));
        await WaitForBlazorReady(page);

        // Basic auth mode: select user from dropdown
        var select = await page.WaitForSelectorAsync("select.form-select",
            new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible, Timeout = DefaultTimeout });

        // Find the option value by evaluating JS
        var optionValue = await page.EvaluateAsync<string?>(
            $@"() => {{
                const options = document.querySelectorAll('select.form-select option');
                for (const opt of options) {{
                    if (opt.textContent.includes('{userDisplayName}')) return opt.getAttribute('value');
                }}
                return null;
            }}");

        if (string.IsNullOrEmpty(optionValue))
            throw new InvalidOperationException($"User '{userDisplayName}' not found in login dropdown.");

        await select!.SelectOptionAsync(new SelectOptionValue { Value = optionValue });

        // Fill password
        await FillPasswordAsync(page, password);

        // Click login
        await ClickLoginButtonAsync(page);

        // Wait for redirect to /main
        await page.WaitForURLAsync("**/main", new PageWaitForURLOptions { Timeout = DefaultTimeout });
    }

    /// <summary>
    /// Вход в Board Portal (Basic auth): выбрать пользователя из выпадающего списка, ввести пароль, кликнуть "Войти".
    /// </summary>
    public static async Task LoginAsBoardUserAsync(IPage page, string userDisplayName, string password = "1")
    {
        await page.GotoAsync(PortalUrls.GetUrl(Portal.BoardPortal, "/login"));
        await WaitForBlazorReady(page);

        var select = await page.WaitForSelectorAsync("select.form-select",
            new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible, Timeout = DefaultTimeout });

        var optionValue = await page.EvaluateAsync<string?>(
            $@"() => {{
                const options = document.querySelectorAll('select.form-select option');
                for (const opt of options) {{
                    if (opt.textContent.includes('{userDisplayName}')) return opt.getAttribute('value');
                }}
                return null;
            }}");

        if (string.IsNullOrEmpty(optionValue))
            throw new InvalidOperationException($"User '{userDisplayName}' not found in login dropdown.");

        await select!.SelectOptionAsync(new SelectOptionValue { Value = optionValue });

        await FillPasswordAsync(page, password);
        await ClickLoginButtonAsync(page);

        await page.WaitForURLAsync("**/main", new PageWaitForURLOptions { Timeout = DefaultTimeout });
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
        var passwordInput = await page.WaitForSelectorAsync(
            "input[type='password'], input.password-input, input[placeholder*='пароль']",
            new PageWaitForSelectorOptions { State = WaitForSelectorState.Visible, Timeout = DefaultTimeout });

        if (passwordInput is null)
            throw new InvalidOperationException("Password input not found.");

        await passwordInput.FillAsync(password);
    }

    private static async Task ClickLoginButtonAsync(IPage page)
    {
        // Find button with text "Войти"
        await page.EvaluateAsync(
            @"() => {
                const buttons = document.querySelectorAll('button.btn-primary');
                for (const btn of buttons) {
                    if (btn.textContent.includes('Войти')) { btn.click(); return; }
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
    /// Дождаться готовности Blazor Server (SignalR connection established).
    /// </summary>
    public static async Task WaitForBlazorReady(IPage page, int timeoutMs = DefaultTimeout)
    {
        await page.WaitForFunctionAsync(
            @"() => {
                const blazorScript = document.querySelector('script[src*=""blazor.server.js""]');
                if (!blazorScript) return document.querySelector('._framework/blazor.server.js') !== null;
                return true;
            }",
            null,
            new PageWaitForFunctionOptions { Timeout = timeoutMs });

        // Give Blazor a moment to establish SignalR connection
        await page.WaitForTimeoutAsync(500);
    }
}
