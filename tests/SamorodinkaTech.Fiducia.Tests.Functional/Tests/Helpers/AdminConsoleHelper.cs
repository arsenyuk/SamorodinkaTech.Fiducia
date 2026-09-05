using Microsoft.Playwright;

namespace SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

/// <summary>
/// Хелпер для взаимодействия с Admin Console (порт 5001).
/// </summary>
public static class AdminConsoleHelper
{
    private const int DefaultTimeout = 15_000;

    /// <summary>
    /// Навигация через меню Admin Console. Кликает по ссылке в левом меню.
    /// </summary>
    public static async Task NavigateToAsync(IPage page, string menuHref)
    {
        // Раскрыть sidebar если collapsed
        var toggler = await page.QuerySelectorAsync("button.navbar-toggler");
        if (toggler != null && await toggler.IsVisibleAsync())
        {
            await toggler.ClickAsync();
            await page.WaitForTimeoutAsync(300);
        }

        var link = page.Locator($"a[href='{menuHref}']");
        if (await link.CountAsync() > 0)
        {
            await link.First.ClickAsync();
        }
        else
        {
            await page.ClickAsync($"a[href='{menuHref}']");
        }
        await AuthHelper.WaitForBlazorReady(page);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Создать пользователя на странице /users через UI.
    /// Использует LDAP-поиск: логин вводится в поле поиска, остальные поля заполняются автоматически.
    /// </summary>
    public static async Task CreateUserViaUiAsync(IPage page, string login, string lastName, string firstName, string middleName, string email)
    {
        await NavigateToAsync(page, "/users");

        // Клик "+ Добавить"
        await page.ClickAsync("button.btn-primary:has-text('Добавить')");
        await page.WaitForTimeoutAsync(500);

        // Дождаться модального окна
        await page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions { Timeout = DefaultTimeout });

        // Ввести логин в поле поиска и нажать 🔍
        var searchInput = await page.QuerySelectorAsync(".modal .input-group input.form-control");
        if (searchInput is not null)
        {
            await searchInput.FillAsync(login);
            await searchInput.DispatchEventAsync("change");
        }

        // Нажать кнопку поиска
        await page.ClickAsync(".modal .input-group button.btn-outline-secondary");

        // Дождаться автозаполнения полей (поле Логин станет readonly и заполненным)
        await page.WaitForFunctionAsync(
            @"() => {
                const inputs = document.querySelectorAll('.modal .modal-body input.form-control[readonly]');
                for (const input of inputs) {
                    if (input.value.length > 0) return true;
                }
                return false;
            }",
            null,
            new PageWaitForFunctionOptions { Timeout = DefaultTimeout });

        // Выбрать роль "Секретарь" (доступна для всех тестовых сценариев)
        await page.SelectOptionAsync(".modal .modal-body select.form-select", "SECRETARY");
        await page.WaitForTimeoutAsync(500);

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
            await NavigateToAsync(page, "/access-management");
        }

        // Дождаться кнопки "+ Создать ЮЛ"
        await page.WaitForSelectorAsync("button:has-text('Создать ЮЛ')", new PageWaitForSelectorOptions { Timeout = DefaultTimeout });

        // Playwright DispatchEventAsync — programmatic click (Playwright docs: input#programmatic-click)
        await page.GetByTestId("show-create-le").DispatchEventAsync("click");
        await page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions { Timeout = DefaultTimeout });

        // Fill + change event для Blazor @bind (@onchange)
        var nameInput = page.GetByTestId("le-name");
        await nameInput.FillAsync(name);
        await nameInput.DispatchEventAsync("change");

        var innInput = page.GetByTestId("le-inn");
        await innInput.FillAsync(inn);
        await innInput.DispatchEventAsync("change");

        // Дополнительно: Tab для надёжного триггера @onchange через потерю фокуса
        await page.Keyboard.PressAsync("Tab");

        // Wait for Blazor to process change events and enable the button
        // Ожидание: кнопка «Создать» станет активной
        await page.WaitForFunctionAsync(
            @"() => {
                const btn = document.querySelector('[data-testid=""le-create""]');
                return btn && !btn.disabled;
            }",
            null,
            new PageWaitForFunctionOptions { Timeout = DefaultTimeout });

        // Click "Создать" — Focus + Enter (keyboard interaction триггерит Blazor @onclick)
        await page.GetByTestId("le-create").FocusAsync();
        await page.Keyboard.PressAsync("Enter");

        // Wait for modal to close
        await page.WaitForFunctionAsync(
            "() => document.querySelector('.modal.show') === null",
            null,
            new PageWaitForFunctionOptions { Timeout = DefaultTimeout });

        // Reload page to refresh entity list, then select the newly created entity
        await NavigateToAsync(page, "/access-management");

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
    /// Гарантирует, что ЮЛ выбрано в dropdown перед добавлением.
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
            await NavigateToAsync(page, "/access-management");
        }

        // Гарантируем, что ЮЛ выбрано (после LoadEmployeesAsync выбор может сброситься)
        await EnsureEntitySelectedAsync(page);

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

    /// <summary>
    /// Гарантировать, что ЮЛ выбрано в dropdown на странице /access-management.
    /// Если уже выбрано — no-op. Если нет — выбрать первое доступное.
    /// </summary>
    private static async Task EnsureEntitySelectedAsync(IPage page)
    {
        var isSelected = await page.EvaluateAsync<bool>(
            @"() => {
                const sel = document.querySelector('.card-body select.form-select');
                return sel && sel.value !== '';
            }");

        if (!isSelected)
        {
            // Выбрать первое доступное ЮЛ
            await page.EvaluateAsync(
                @"() => {
                    const sel = document.querySelector('.card-body select.form-select');
                    if (sel && sel.options.length > 1) {
                        sel.value = sel.options[1].value;
                        sel.dispatchEvent(new Event('change', { bubbles: true }));
                    }
                }");
            await page.WaitForTimeoutAsync(1000);
        }
    }

    /// <summary>
    /// Установить ОКОПФ для юридического лица через API (PUT /api/legal-entities/{id}/okopf).
    /// Требует авторизованную сессию Admin Console с ролью SYS_ADMIN.
    /// </summary>
    public static async Task SetOkopfAsync(IPage page, Guid legalEntityId, string okopfCode)
    {
        var result = await page.EvaluateAsync<dynamic>(
            $@"async () => {{
                const response = await fetch('/api/legal-entities/{legalEntityId}/okopf?okopfCode={okopfCode}', {{
                    method: 'PUT',
                    credentials: 'same-origin'
                }});
                if (!response.ok) {{
                    const body = await response.text();
                    throw new Error(`PUT /api/legal-entities/{legalEntityId}/okopf failed: ${{response.status}} ${{body}}`);
                }}
                return await response.json();
            }}");
    }

    private static string EscapeJs(string value) => value.Replace("'", "\\'").Replace("\\", "\\\\");
}
