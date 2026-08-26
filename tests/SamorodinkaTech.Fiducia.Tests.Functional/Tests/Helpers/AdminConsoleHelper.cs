using Microsoft.Playwright;

namespace SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

/// <summary>
/// Хелпер для взаимодействия с Admin Console (порт 5001).
/// </summary>
public static class AdminConsoleHelper
{
    private const int DefaultTimeout = 15_000;

    /// <summary>
    /// Создать юридическое лицо на странице /access-management.
    /// </summary>
    public static async Task CreateLegalEntityAsync(IPage page, string name, string inn)
    {
        await page.GotoAsync(PortalUrls.GetUrl(Portal.AdminConsole, "/access-management"));
        await AuthHelper.WaitForBlazorReady(page);
        await page.WaitForTimeoutAsync(1000);

        // Click "+ Создать ЮЛ" button via JS
        await page.EvaluateAsync(
            @"() => {
                const buttons = document.querySelectorAll('button');
                for (const btn of buttons) {
                    if (btn.textContent.includes('Создать ЮЛ')) { btn.click(); return; }
                }
            }");
        await page.WaitForTimeoutAsync(500);

        // Wait for modal to appear
        await page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions { Timeout = DefaultTimeout });

        // Fill name — @bind uses 'change' event, 'input' only triggers @oninput
        await page.EvaluateAsync(
            $@"() => {{
                const inputs = document.querySelectorAll('.modal input.form-control');
                if (inputs.length > 0) {{
                    const nameInput = inputs[0];
                    nameInput.value = '{name.Replace("'", "\\'")}';
                    nameInput.dispatchEvent(new Event('input', {{ bubbles: true }}));
                    nameInput.dispatchEvent(new Event('change', {{ bubbles: true }}));
                }}
            }}");

        // Fill INN — same: must dispatch 'change' for @bind to pick up the value
        await page.EvaluateAsync(
            $@"() => {{
                const inputs = document.querySelectorAll('.modal input.form-control');
                for (const input of inputs) {{
                    if (input.maxLength === 12 || input.getAttribute('maxlength') === '12') {{
                        input.value = '{inn}';
                        input.dispatchEvent(new Event('input', {{ bubbles: true }}));
                        input.dispatchEvent(new Event('change', {{ bubbles: true }}));
                        return;
                    }}
                }}
            }}");

        // Wait for Blazor to process change events and enable the button
        await page.WaitForFunctionAsync(
            @"() => {
                const btn = document.querySelector('.modal-footer button.btn-primary');
                return btn && !btn.disabled;
            }",
            null,
            new PageWaitForFunctionOptions { Timeout = DefaultTimeout });

        // Click "Создать" in modal
        await page.EvaluateAsync(
            @"() => {
                const buttons = document.querySelectorAll('.modal-footer button');
                for (const btn of buttons) {
                    if (btn.textContent.includes('Создать')) { btn.click(); return; }
                }
            }");

        // Wait for modal to close
        await page.WaitForFunctionAsync(
            "() => document.querySelector('.modal.show') === null",
            null,
            new PageWaitForFunctionOptions { Timeout = DefaultTimeout });

        // Reload page to refresh entity list, then select the newly created entity
        await page.ReloadAsync();
        await AuthHelper.WaitForBlazorReady(page);

        // Wait for entity list to load (select must have >1 option)
        await page.WaitForFunctionAsync(
            @"() => {
                const sel = document.querySelector('.card-body select.form-select');
                return sel && sel.options.length > 1;
            }",
            null,
            new PageWaitForFunctionOptions { Timeout = DefaultTimeout });

        // Get option value by matching INN in label text, then use Playwright SelectOption
        var optionValue = await page.EvaluateAsync<string?>(
            $@"() => {{
                const sel = document.querySelector('.card-body select.form-select');
                if (!sel) return null;
                for (const opt of sel.options) {{
                    if (opt.text.includes('{inn}')) return opt.value;
                }}
                return null;
            }}");

        if (optionValue is not null)
        {
            await page.SelectOptionAsync(".card-body select.form-select", optionValue);
        }

        await page.WaitForTimeoutAsync(500);
    }

    /// <summary>
    /// Добавить сотрудника в ЮЛ на странице /access-management.
    /// </summary>
    public static async Task AddEmployeeAsync(
        IPage page,
        string lastName,
        string firstName,
        string middleName,
        string position,
        string login,
        string roleCode)
    {
        // Ensure we're on the access management page
        if (!page.Url.Contains("/access-management"))
        {
            await page.GotoAsync(PortalUrls.GetUrl(Portal.AdminConsole, "/access-management"));
            await AuthHelper.WaitForBlazorReady(page);
            await page.WaitForTimeoutAsync(1000);
        }

        // Fill all form fields using JS (compact layout with form-control-sm)
        // @bind uses 'change' event — must dispatch it for C# backing fields to update
        await page.EvaluateAsync(
            $@"() => {{
                const inputs = document.querySelectorAll('.card-body .form-control-sm');
                // Fields order: LastName, FirstName, MiddleName, Position, Login
                const values = ['{EscapeJs(lastName)}', '{EscapeJs(firstName)}', '{EscapeJs(middleName)}', '{EscapeJs(position)}', '{EscapeJs(login)}'];
                for (let i = 0; i < Math.min(inputs.length, values.length); i++) {{
                    if (inputs[i].tagName === 'INPUT') {{
                        inputs[i].value = values[i];
                        inputs[i].dispatchEvent(new Event('input', {{ bubbles: true }}));
                        inputs[i].dispatchEvent(new Event('change', {{ bubbles: true }}));
                    }}
                }}
            }}");

        // Select role — use Playwright SelectOption for reliable @bind update
        var roleSelectSelector = ".form-select-sm";
        await page.WaitForFunctionAsync(
            @"() => {
                const selects = document.querySelectorAll('.form-select-sm');
                return selects.length > 0;
            }",
            null,
            new PageWaitForFunctionOptions { Timeout = DefaultTimeout });

        await page.SelectOptionAsync(roleSelectSelector, roleCode);

        // Wait for Blazor to process change events and enable the button
        await page.WaitForFunctionAsync(
            @"() => {
                const btn = document.querySelector('button.btn-primary.btn-sm');
                return btn && !btn.disabled;
            }",
            null,
            new PageWaitForFunctionOptions { Timeout = DefaultTimeout });

        // Click "Добавить" button
        await page.EvaluateAsync(
            @"() => {
                const buttons = document.querySelectorAll('button');
                for (const btn of buttons) {
                    if (btn.textContent.includes('Добавить') && btn.classList.contains('btn-primary')) {
                        btn.click(); return;
                    }
                }
            }");

        // Wait for processing
        await page.WaitForTimeoutAsync(1000);
    }

    /// <summary>
    /// Назначить пользователю роли в ЮЛ.
    /// </summary>
    public static async Task AssignRolesAsync(
        IPage page,
        string lastName,
        string firstName,
        string middleName,
        string position,
        string login,
        string[] roleCodes)
    {
        foreach (var roleCode in roleCodes)
        {
            await AddEmployeeAsync(page, lastName, firstName, middleName, position, login, roleCode);
        }
    }

    private static string EscapeJs(string value) => value.Replace("'", "\\'").Replace("\\", "\\\\");
}
