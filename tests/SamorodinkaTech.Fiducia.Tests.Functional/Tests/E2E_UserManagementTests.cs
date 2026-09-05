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
    /// Администратор вводит несуществующий логин, нажимает поиск.
    /// Ожидается: предупреждение «не найден в LDAP» + кнопка «Создать» неактивна.
    /// </summary>
    [Fact]
    public async Task AdminConsole_CreateUser_LdapSearchNotFound_ShowWarningAndButtonDisabled()
    {
        var page = await CreateAdminConsolePageAsync();
        await AuthHelper.LoginAsAdminAsync(page, "admin", "1");

        // Навигация на /users через меню
        await AdminConsoleHelper.NavigateToAsync(page, "/users");

        // Открыть модальное окно создания пользователя
        await page.ClickAsync("button.btn-primary:has-text('Добавить')");
        await page.WaitForSelectorAsync(".modal.show", new() { Timeout = DefaultTimeout });

        // Ввести несуществующий логин
        var loginInput = page.Locator(".modal .input-group input.form-control");
        await loginInput.FillAsync("nonexistent_user_xyz_999");
        await loginInput.DispatchEventAsync("change");

        // Нажать кнопку поиска (🔍)
        await page.ClickAsync(".modal .input-group button.btn-outline-secondary");

        // Дождаться предупреждения «не найден в LDAP»
        await page.WaitForSelectorAsync(".modal .text-warning", new() { Timeout = DefaultTimeout });
        var warningText = await page.EvalOnSelectorAsync<string>(".modal .text-warning", "el => el.textContent");
        warningText.Should().Contain("не найден в LDAP",
            "При ненайденном LDAP-пользователе должно отображаться предупреждение");

        // Проверить: кнопка «Создать» неактивна
        var createButton = page.Locator(".modal-footer button.btn-primary");
        var isDisabled = await createButton.GetAttributeAsync("disabled");
        isDisabled.Should().NotBeNull(
            "Кнопка «Создать» должна быть disabled при ненайденном LDAP-пользователе");
    }

    /// <summary>
    /// Администратор вводит существующий логин, нажимает поиск.
    /// Ожидается: поля автозаполняются + кнопка «Создать» активна.
    /// </summary>
    [Fact]
    public async Task AdminConsole_CreateUser_LdapSearchFound_FieldsAutoFilledButtonEnabled()
    {
        var page = await CreateAdminConsolePageAsync();
        await AuthHelper.LoginAsAdminAsync(page, "admin", "1");

        // Навигация на /users через меню
        await AdminConsoleHelper.NavigateToAsync(page, "/users");

        // Открыть модальное окно создания пользователя
        await page.ClickAsync("button.btn-primary:has-text('Добавить')");
        await page.WaitForSelectorAsync(".modal.show", new() { Timeout = DefaultTimeout });

        // Ввести существующий логин (LDAP-пользователь из seed-данных)
        var loginInput = page.Locator(".modal .input-group input.form-control");
        await loginInput.FillAsync("admin");
        await loginInput.DispatchEventAsync("change");

        // Нажать кнопку поиска (🔍)
        await page.ClickAsync(".modal .input-group button.btn-outline-secondary");

        // Дождаться автозаполнения полей (LDAP вернул данные)
        await page.WaitForFunctionAsync(
            @"() => {
                const inputs = document.querySelectorAll('.modal .modal-body input.form-control');
                // Ищем input с заполненной фамилией (индекс 1 после логина)
                for (const input of inputs) {
                    const parent = input.parentElement;
                    if (parent && !parent.classList.contains('input-group') && input.value.length > 0) {
                        return true;
                    }
                }
                return false;
            }",
            null,
            new() { Timeout = DefaultTimeout });

        // Проверить: предупреждения нет
        var warningLocator = page.Locator(".modal .text-warning");
        var warningCount = await warningLocator.CountAsync();
        warningCount.Should().Be(0,
            "При успешном LDAP-поиске предупреждений быть не должно");

        // Проверить: кнопка «Создать» активна (атрибут disabled отсутствует)
        var createButton = page.Locator(".modal-footer button.btn-primary");
        var isDisabled = await createButton.GetAttributeAsync("disabled");
        isDisabled.Should().BeNull(
            "Кнопка «Создать» должна быть активной при найденном LDAP-пользователе");
    }
}
