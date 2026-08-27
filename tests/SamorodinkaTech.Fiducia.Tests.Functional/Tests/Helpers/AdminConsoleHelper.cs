using Microsoft.Playwright;

namespace SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

/// <summary>
/// Хелпер для взаимодействия с Admin Console (порт 5001).
/// </summary>
public static class AdminConsoleHelper
{
    private const int DefaultTimeout = 15_000;

    /// <summary>
    /// Создать пользователя на странице /users через UI.
    /// </summary>
    public static async Task CreateUserViaUiAsync(IPage page, string login, string lastName, string firstName, string middleName, string email)
    {
        await page.GotoAsync(PortalUrls.GetUrl(Portal.AdminConsole, "/users"));
        await AuthHelper.WaitForBlazorReady(page);
        await page.WaitForTimeoutAsync(2000);

        // Клик "+ Добавить"
        await page.ClickAsync("button.btn-primary:has-text('Добавить')");
        await page.WaitForTimeoutAsync(500);

        // Дождаться модального окна
        await page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions { Timeout = DefaultTimeout });

        // Заполнить поле Логин
        var loginInput = await page.QuerySelectorAsync(".modal .input-group input.form-control");
        if (loginInput is not null)
        {
            await loginInput.FillAsync(login);
            await loginInput.DispatchEventAsync("change");
        }

        // Заполнить остальные поля (пропуская уже заполненное поле логина в input-group)
        var allInputs = await page.QuerySelectorAllAsync(".modal .modal-body input.form-control");
        var values = new[] { lastName, firstName, middleName, email };
        var valueIndex = 0;
        foreach (var input in allInputs)
        {
            var parentHtml = await input.EvaluateAsync<string>("el => el.parentElement?.className ?? ''");
            if (parentHtml.Contains("input-group"))
                continue; // поле логина уже заполнено — пропускаем

            if (input is not null && valueIndex < values.Length && !string.IsNullOrEmpty(values[valueIndex]))
            {
                await input.FillAsync(values[valueIndex]);
                await input.DispatchEventAsync("change");
            }
            valueIndex++;
        }

        await page.WaitForTimeoutAsync(500);

        // Клик "Создать"
        await page.ClickAsync(".modal-footer button.btn-primary");

        // Дождаться закрытия модального окна
        await page.WaitForFunctionAsync(
            "() => document.querySelector('.modal.show') === null",
            null,
            new PageWaitForFunctionOptions { Timeout = DefaultTimeout });

        await page.WaitForTimeoutAsync(1000);
    }

    /// <summary>
    /// Создать юридическое лицо на странице /access-management.
    /// </summary>
    public static async Task CreateLegalEntityAsync(IPage page, string name, string inn)
    {
        if (!page.Url.Contains("/access-management"))
        {
            await page.GotoAsync(PortalUrls.GetUrl(Portal.AdminConsole, "/access-management"));
            await AuthHelper.WaitForBlazorReady(page);
            await page.WaitForTimeoutAsync(3000);
        }

        // Дождаться кнопки "+ Создать ЮЛ"
        await page.WaitForSelectorAsync("button:has-text('Создать ЮЛ')", new PageWaitForSelectorOptions { Timeout = DefaultTimeout });

        // Click "+ Создать ЮЛ" button
        await page.ClickAsync("button:has-text('Создать ЮЛ')");
        await page.WaitForTimeoutAsync(500);

        // Wait for modal to appear
        await page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions { Timeout = DefaultTimeout });

        // Fill name — FillAsync dispatches 'input' but Blazor @bind needs 'change'
        var nameInput = await page.QuerySelectorAsync(".modal input.form-control");
        if (nameInput is not null)
        {
            await nameInput.FillAsync(name);
            await nameInput.DispatchEventAsync("change");
        }

        // Fill INN — find input with maxlength=12
        var allModalInputs = await page.QuerySelectorAllAsync(".modal input.form-control");
        foreach (var input in allModalInputs)
        {
            var maxLength = await input.GetAttributeAsync("maxlength");
            if (maxLength == "12")
            {
                await input.FillAsync(inn);
                await input.DispatchEventAsync("change");
                break;
            }
        }

        // Wait for Blazor to process change events and enable the button
        await page.WaitForFunctionAsync(
            @"() => {
                const btn = document.querySelector('.modal-footer button.btn-primary');
                return btn && !btn.disabled;
            }",
            null,
            new PageWaitForFunctionOptions { Timeout = DefaultTimeout });

        // Click "Создать" in modal
        await page.ClickAsync(".modal-footer button.btn-primary");

        // Wait for modal to close
        await page.WaitForFunctionAsync(
            "() => document.querySelector('.modal.show') === null",
            null,
            new PageWaitForFunctionOptions { Timeout = DefaultTimeout });

        // Reload page to refresh entity list, then select the newly created entity
        await page.GotoAsync(PortalUrls.GetUrl(Portal.AdminConsole, "/access-management"));
        await AuthHelper.WaitForBlazorReady(page);
        await page.WaitForTimeoutAsync(3000);

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
            await page.WaitForTimeoutAsync(3000);
        }

        // Wait for employee form to be visible (depends on _selectedLegalEntityId)
        try
        {
            await page.WaitForSelectorAsync(".card-body input.form-control-sm, .card-body .input-group .form-control", new PageWaitForSelectorOptions { Timeout = DefaultTimeout });
        }
        catch (TimeoutException)
        {
            // Диагностика: вывести состояние страницы
            var url = page.Url;
            var bodyText = await page.EvaluateAsync<string>("() => document.body?.innerText?.substring(0, 500) ?? 'empty'");
            throw new InvalidOperationException(
                $"[AddEmployeeAsync] Форма сотрудника не найдена. URL: {url}. Body: {bodyText}");
        }

        // Fill fields — LastName, FirstName, MiddleName, Position have form-control-sm; Login has form-control inside input-group
        var nameInputs = await page.QuerySelectorAllAsync(".card-body input.form-control-sm");
        var nameValues = new[] { lastName, firstName, middleName, position };
        for (int i = 0; i < Math.Min(nameInputs.Count, nameValues.Length); i++)
        {
            if (nameInputs[i] is not null)
            {
                await nameInputs[i].FillAsync(nameValues[i]);
                await nameInputs[i].DispatchEventAsync("change");
            }
        }

        // Login is in .input-group with class "form-control" (not form-control-sm)
        var loginInput = await page.QuerySelectorAsync(".card-body .input-group .form-control");
        if (loginInput is not null)
        {
            await loginInput.FillAsync(login);
            await loginInput.DispatchEventAsync("change");
        }

        // Select role — last .form-select-sm on the page
        await page.SelectOptionAsync(".card-body .form-select-sm", roleCode);
        await page.WaitForTimeoutAsync(500);

        // Wait for Blazor to process all change events and enable the button
        await page.WaitForFunctionAsync(
            @"() => {
                const btn = document.querySelector('button.btn-primary.btn-sm');
                return btn && !btn.disabled;
            }",
            null,
            new PageWaitForFunctionOptions { Timeout = DefaultTimeout });

        // Click "Добавить" button
        await page.ClickAsync("button.btn-primary.btn-sm");

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
