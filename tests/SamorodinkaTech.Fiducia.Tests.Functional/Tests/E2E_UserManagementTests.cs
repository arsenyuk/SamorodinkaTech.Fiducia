using FluentAssertions;
using Microsoft.Playwright;
using SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

namespace SamorodinkaTech.Fiducia.Tests.Functional;

/// <summary>
/// E2E-тесты управления пользователями в Admin Console.
/// </summary>
public class E2E_UserManagementTests : BrowserFixture
{
    private const int DefaultTimeout = 15_000;

    public E2E_UserManagementTests(GlobalFixture globalFixture) : base(globalFixture)
    {
    }

    /// <summary>
    /// Ввод несуществующего логина → warning + кнопка «Создать» неактивна.
    /// </summary>
    [Fact]
    public async Task CreateUser_LdapNotFound_ShowsWarningAndButtonDisabled()
    {
        var page = await CreateAdminConsolePageAsync();
        await AuthHelper.LoginAsAdminAsync(page, "admin", "1");
        await AdminConsoleHelper.NavigateToAsync(page, "/users");

        // Открыть модальное окно
        await page.ClickAsync("button.btn-primary:has-text('Добавить')");
        await page.WaitForSelectorAsync(".modal.show", new() { Timeout = DefaultTimeout });

        // Ввести несуществующий логин в поле поиска
        var searchInput = page.Locator(".modal .input-group input.form-control");
        await searchInput.FillAsync("nonexistent_user_xyz_999");
        await searchInput.DispatchEventAsync("change");

        // Нажать 🔍
        await page.ClickAsync(".modal .input-group button.btn-outline-secondary");

        // Дождаться предупреждения
        await page.WaitForSelectorAsync(".modal .text-warning", new() { Timeout = DefaultTimeout });
        var warningText = await page.EvalOnSelectorAsync<string>(".modal .text-warning", "el => el.textContent");
        warningText.Should().Contain("не найден в LDAP");

        // Кнопка «Создать» неактивна
        var createButton = page.Locator(".modal-footer button.btn-primary");
        var isDisabled = await createButton.GetAttributeAsync("disabled");
        isDisabled.Should().NotBeNull("Кнопка «Создать» должна быть disabled при ненайденном LDAP-пользователе");
    }

    /// <summary>
    /// LDAP найден, но роль не выбрана → кнопка «Создать» неактивна.
    /// </summary>
    [Fact]
    public async Task CreateUser_LdapFoundButNoRole_ButtonDisabled()
    {
        var page = await CreateAdminConsolePageAsync();
        await AuthHelper.LoginAsAdminAsync(page, "admin", "1");
        await AdminConsoleHelper.NavigateToAsync(page, "/users");

        await page.ClickAsync("button.btn-primary:has-text('Добавить')");
        await page.WaitForSelectorAsync(".modal.show", new() { Timeout = DefaultTimeout });

        // Ввести существующий логин
        var searchInput = page.Locator(".modal .input-group input.form-control");
        await searchInput.FillAsync("admin");
        await searchInput.DispatchEventAsync("change");

        await page.ClickAsync(".modal .input-group button.btn-outline-secondary");

        // Дождаться автозаполнения полей (поле Логин заполнено)
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

        // Кнопка «Создать» неактивна (роль не выбрана)
        var createButton = page.Locator(".modal-footer button.btn-primary");
        var isDisabled = await createButton.GetAttributeAsync("disabled");
        isDisabled.Should().NotBeNull("Кнопка «Создать» должна быть disabled без выбранной роли");
    }

    /// <summary>
    /// LDAP найден + роль выбрана → кнопка «Создать» активна.
    /// </summary>
    [Fact]
    public async Task CreateUser_LdapFoundAndRoleSelected_ButtonEnabled()
    {
        var page = await CreateAdminConsolePageAsync();
        await AuthHelper.LoginAsAdminAsync(page, "admin", "1");
        await AdminConsoleHelper.NavigateToAsync(page, "/users");

        await page.ClickAsync("button.btn-primary:has-text('Добавить')");
        await page.WaitForSelectorAsync(".modal.show", new() { Timeout = DefaultTimeout });

        // Ввести существующий логин
        var searchInput = page.Locator(".modal .input-group input.form-control");
        await searchInput.FillAsync("admin");
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

        // Выбрать роль «Администратор ЮЛ»
        await page.SelectOptionAsync(".modal .modal-body select.form-select", "LE_ADMIN");
        await page.WaitForTimeoutAsync(500);

        // Кнопка «Создать» активна
        var createButton = page.Locator(".modal-footer button.btn-primary");
        var isDisabled = await createButton.GetAttributeAsync("disabled");
        isDisabled.Should().BeNull("Кнопка «Создать» должна быть активной при LDAP-найденном пользователе и выбранной роли");
    }
}
