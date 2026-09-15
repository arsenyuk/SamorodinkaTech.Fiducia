using Microsoft.Playwright;
using SamorodinkaTech.Fiducia.Tests.Functional;

namespace SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

/// <summary>
/// Хелпер для взаимодействия с Admin Console (порт 5001).
/// </summary>
public static class AdminConsoleHelper
{
    private static int DefaultTimeout => GlobalFixture.TestOptions.TimeoutMs;

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
    /// Создать юридическое лицо на странице /legal-entities и перейти к нему.
    /// </summary>
    public static async Task CreateLegalEntityAsync(IPage page, string name, string inn)
    {
        // Навигация на страницу Общества
        await page.GotoAsync(PortalUrls.GetUrl(Portal.AdminConsole, "/legal-entities"));
        await AuthHelper.WaitForBlazorReady(page);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Дождаться кнопки "+ Создать ЮЛ"
        await page.WaitForSelectorAsync("button:has-text('Создать ЮЛ')", new PageWaitForSelectorOptions { Timeout = DefaultTimeout });
        await page.ClickAsync("button:has-text('Создать ЮЛ')");

        await page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions { Timeout = DefaultTimeout });

        // Fill + change event для Blazor @bind (@onchange)
        var nameInput = page.GetByTestId("le-name");
        if (await nameInput.CountAsync() == 0)
            throw new InvalidOperationException("Не найдено поле «Наименование ЮЛ» (data-testid=le-name) в модалке создания ЮЛ");
        await nameInput.FillAsync(name);
        await nameInput.DispatchEventAsync("change");

        var innInput = page.GetByTestId("le-inn");
        if (await innInput.CountAsync() == 0)
            throw new InvalidOperationException("Не найдено поле «ИНН» (data-testid=le-inn) в модалке создания ЮЛ");
        await innInput.FillAsync(inn);
        await innInput.DispatchEventAsync("change");

        // Дополнительно: Tab для надёжного триггера @onchange через потерю фокуса
        await page.Keyboard.PressAsync("Tab");

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

        // Перейти к /access-management для newly created LE
        // Ищем созданное ЮЛ в таблице по ИНН, кликаем по нему
        await page.WaitForSelectorAsync("tbody tr", new PageWaitForSelectorOptions { Timeout = DefaultTimeout });

        // Клик по строке таблицы, содержащей ИНН
        var rowClicked = await page.EvaluateAsync<bool>(
            $@"() => {{
                const rows = document.querySelectorAll('tbody tr');
                for (const row of rows) {{
                    if (row.textContent.includes('{inn}')) {{
                        row.click();
                        return true;
                    }}
                }}
                return false;
            }}");

        if (rowClicked)
        {
            await AuthHelper.WaitForBlazorReady(page);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        await page.WaitForTimeoutAsync(500);
    }

    /// <summary>
    /// Добавить сотрудника в ЮЛ на странице /access-management.
    /// Использует модальный диалог: поиск по логину → LDAP → выбор роли → «Добавить».
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
        // Если страница уже на /access-management?le= — просто ждём загрузки
        if (!page.Url.Contains("/access-management?le="))
        {
            // Не на странице — нужна навигация. Но откуда взять ID?
            // Бросаем ошибку — вызывающий код должен обеспечить навигацию.
            throw new InvalidOperationException(
                "[AdminConsoleHelper] AddEmployeeAsync: страница не на /access-management?le={id}. " +
                "Вызовите CreateLegalEntityAsync или FindAndNavigateToLegalEntityAsync перед AddEmployeeAsync.");
        }

        await AuthHelper.WaitForBlazorReady(page);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Имя ЮЛ уже отображается на странице в .card-body strong

        // Дождаться загрузки страницы (кнопка «Добавить сотрудника»)
        await page.WaitForSelectorAsync("button.btn-primary.btn-sm:has-text('Добавить сотрудника')", new PageWaitForSelectorOptions { Timeout = DefaultTimeout });

        // Click "+ Добавить сотрудника"
        await page.ClickAsync("button.btn-primary.btn-sm:has-text('Добавить сотрудника')");
        await page.WaitForSelectorAsync(".modal.show", new PageWaitForSelectorOptions { Timeout = DefaultTimeout });

        // Enter login in search field and click 🔍
        var searchInput = page.Locator(".modal .input-group input.form-control");
        await searchInput.FillAsync(login);
        await searchInput.DispatchEventAsync("change");

        await page.ClickAsync(".modal .input-group button.btn-outline-secondary");

        // Check for duplicate login warning — if exists, close modal and return
        await page.WaitForTimeoutAsync(1000);
        var duplicateWarning = await page.QuerySelectorAsync(".modal .text-warning");
        if (duplicateWarning is not null)
        {
            // Close modal
            var closeBtn = await page.QuerySelectorAsync(".modal .btn-close");
            if (closeBtn is not null) await closeBtn.ClickAsync();
            await page.WaitForTimeoutAsync(500);
            return;
        }

        // Wait for LDAP to populate readonly fields
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

        // Wait for role dropdown to be populated (options loaded from DB)
        await page.WaitForFunctionAsync(
            @"() => {
                const sel = document.querySelector('.modal .modal-body select.form-select');
                return sel && sel.options.length > 1;
            }",
            null,
            new PageWaitForFunctionOptions { Timeout = DefaultTimeout });

        // Select role
        await page.SelectOptionAsync(".modal .modal-body select.form-select", roleCode);
        await page.WaitForTimeoutAsync(500);

        // Click "Добавить" in modal footer
        await page.ClickAsync(".modal-footer button.btn-primary");

        // Wait for modal to close
        await page.WaitForFunctionAsync(
            "() => document.querySelector('.modal.show') === null",
            null,
            new PageWaitForFunctionOptions { Timeout = DefaultTimeout });

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
    /// Перейти на страницу сотрудников конкретного ЮЛ.
    /// Ищет ЮЛ по имени в «Общества», кликает → переход на /access-management?le={id}.
    /// </summary>
    public static async Task NavigateToLegalEntityAsync(IPage page, string legalEntityName)
    {
        var leId = await FindLegalEntityIdByNameAsync(page, legalEntityName);
        if (leId is null)
        {
            throw new InvalidOperationException(
                $"[AdminConsoleHelper] Юридическое лицо «{legalEntityName}» не найдено в списке «Общества».");
        }
        await AuthHelper.WaitForBlazorReady(page);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Найти ID юридического лица по имени на странице /legal-entities.
    /// </summary>
    private static async Task<string?> FindLegalEntityIdByNameAsync(IPage page, string legalEntityName)
    {
        await page.GotoAsync(PortalUrls.GetUrl(Portal.AdminConsole, "/legal-entities"));
        await AuthHelper.WaitForBlazorReady(page);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await page.WaitForSelectorAsync("tbody tr", new PageWaitForSelectorOptions { Timeout = DefaultTimeout });

        // Извлекаем ID из onclick-атрибута Blazor (data属性)
        // Blazor генерирует onclick с NavigateTo(...), но проще — кликнуть и дождаться URL
        var rowFound = await page.EvaluateAsync<bool>(
            $@"() => {{
                const rows = document.querySelectorAll('tbody tr');
                for (const row of rows) {{
                    if (row.textContent.includes('{EscapeJs(legalEntityName)}')) {{
                        row.click();
                        return true;
                    }}
                }}
                return false;
            }}");

        if (!rowFound) return null;

        // Ждём навигации на /access-management
        try
        {
            await page.WaitForURLAsync("**/access-management**", new PageWaitForURLOptions { Timeout = DefaultTimeout });
        }
        catch
        {
            return null;
        }

        // Извлекаем le из URL
        var url = page.Url;
        if (url.Contains("le="))
        {
            var leParam = url.Split("le=").Last().Split('&')[0];
            return leParam;
        }
        return null;
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
