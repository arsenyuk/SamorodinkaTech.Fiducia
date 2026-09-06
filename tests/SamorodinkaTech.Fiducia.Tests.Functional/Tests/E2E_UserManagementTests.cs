using FluentAssertions;
using Microsoft.Playwright;
using SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

namespace SamorodinkaTech.Fiducia.Tests.Functional;

/// <summary>
/// E2E-тесты добавления сотрудника через /access-management.
/// </summary>
[Collection("E2ETests")]
public class E2E_UserManagementTests : BrowserFixture
{
    private const int DefaultTimeout = 15_000;

    public E2E_UserManagementTests(GlobalFixture globalFixture) : base(globalFixture)
    {
    }

    /// <summary>
    /// Ввод несуществующего логина → warning + кнопка «Добавить» неактивна.
    /// </summary>
    [Fact]
    public async Task AddEmployee_LdapNotFound_ShowsWarningAndButtonDisabled()
    {
        SkipIfPreviousFailed();
        try
        {
        var page = await CreateAdminConsolePageAsync();
        await AuthHelper.LoginAsAdminAsync(page, "v.vasilyeva", "1");

        // Создать тестовое ЮЛ
        await AdminConsoleHelper.CreateLegalEntityAsync(page, "Тестовое ЮЛ", "7701234567");

        // Открыть модалку «Добавить сотрудника»
        await page.ClickAsync("button.btn-primary.btn-sm:has-text('Добавить сотрудника')");
        await page.WaitForSelectorAsync(".modal.show", new() { Timeout = DefaultTimeout });

        // Ввести несуществующий логин
        var searchInput = page.Locator(".modal .input-group input.form-control");
        await searchInput.FillAsync("nonexistent_user_xyz_999");
        await searchInput.DispatchEventAsync("change");

        // Нажать 🔍
        await page.ClickAsync(".modal .input-group button.btn-outline-secondary");

        // Дождаться предупреждения
        await page.WaitForSelectorAsync(".modal .text-warning", new() { Timeout = DefaultTimeout });
        var warningText = await page.EvalOnSelectorAsync<string>(".modal .text-warning", "el => el.textContent");
        warningText.Should().Contain("не найден в LDAP");

        // Кнопка «Добавить» неактивна
        var createButton = page.Locator(".modal-footer button.btn-primary");
        var isDisabled = await createButton.GetAttributeAsync("disabled");
        isDisabled.Should().NotBeNull("Кнопка «Добавить» должна быть disabled при ненайденном LDAP-пользователе");
        }
        catch
        {
            GlobalFixture.MarkFailed();
            throw;
        }
    }

    /// <summary>
    /// LDAP найден, но роль не выбрана → кнопка «Добавить» неактивна.
    /// </summary>
    [Fact]
    public async Task AddEmployee_LdapFoundButNoRole_ButtonDisabled()
    {
        SkipIfPreviousFailed();
        try
        {
        var page = await CreateAdminConsolePageAsync();
        await AuthHelper.LoginAsAdminAsync(page, "v.vasilyeva", "1");

        // Создать тестовое ЮЛ
        await AdminConsoleHelper.CreateLegalEntityAsync(page, "Тестовое ЮЛ 2", "7701234568");

        // Открыть модалку
        await page.ClickAsync("button.btn-primary.btn-sm:has-text('Добавить сотрудника')");
        await page.WaitForSelectorAsync(".modal.show", new() { Timeout = DefaultTimeout });

        // Ввести существующий логин
        var searchInput = page.Locator(".modal .input-group input.form-control");
        await searchInput.FillAsync("nechaev.va");
        await searchInput.DispatchEventAsync("change");

        await page.ClickAsync(".modal .input-group button.btn-outline-secondary");

        // Дождаться автозаполнения
        await page.WaitForFunctionAsync(
            @"() => {
                const inputs = document.querySelectorAll('.modal .modal-body input.form-control[readonly]');
                for (const input of inputs) {
                    if (input.value.length > 0) return true;
                }
                return false;
            }",
            null,
            new() { Timeout = DefaultTimeout });

        // Предупреждений нет
        var warningCount = await page.Locator(".modal .text-warning").CountAsync();
        warningCount.Should().Be(0);

        // Кнопка «Добавить» неактивна (роль не выбрана)
        var createButton = page.Locator(".modal-footer button.btn-primary");
        var isDisabled = await createButton.GetAttributeAsync("disabled");
        isDisabled.Should().NotBeNull("Кнопка «Добавить» должна быть disabled без выбранной роли");
        }
        catch
        {
            GlobalFixture.MarkFailed();
            throw;
        }
    }

    /// <summary>
    /// LDAP найден + роль выбрана → кнопка «Добавить» активна.
    /// </summary>
    [Fact]
    public async Task AddEmployee_LdapFoundAndRoleSelected_ButtonEnabled()
    {
        SkipIfPreviousFailed();
        try
        {
        var page = await CreateAdminConsolePageAsync();
        await AuthHelper.LoginAsAdminAsync(page, "v.vasilyeva", "1");

        // Создать тестовое ЮЛ
        await AdminConsoleHelper.CreateLegalEntityAsync(page, "Тестовое ЮЛ 3", "7701234569");

        // Открыть модалку
        await page.ClickAsync("button.btn-primary.btn-sm:has-text('Добавить сотрудника')");
        await page.WaitForSelectorAsync(".modal.show", new() { Timeout = DefaultTimeout });

        // Ввести существующий логин
        var searchInput = page.Locator(".modal .input-group input.form-control");
        await searchInput.FillAsync("nechaev.va");
        await searchInput.DispatchEventAsync("change");

        await page.ClickAsync(".modal .input-group button.btn-outline-secondary");

        // Дождаться автозаполнения
        await page.WaitForFunctionAsync(
            @"() => {
                const inputs = document.querySelectorAll('.modal .modal-body input.form-control[readonly]');
                for (const input of inputs) {
                    if (input.value.length > 0) return true;
                }
                return false;
            }",
            null,
            new() { Timeout = DefaultTimeout });

        // Выбрать роль
        await page.SelectOptionAsync(".modal .modal-body select.form-select", "LE_ADMIN");
        await page.WaitForTimeoutAsync(500);

        // Кнопка «Добавить» активна
        var createButton = page.Locator(".modal-footer button.btn-primary");
        var isDisabled = await createButton.GetAttributeAsync("disabled");
        isDisabled.Should().BeNull("Кнопка «Добавить» должна быть активной при LDAP-найденном пользователе и выбранной роли");
        }
        catch
        {
            GlobalFixture.MarkFailed();
            throw;
        }
    }
}
