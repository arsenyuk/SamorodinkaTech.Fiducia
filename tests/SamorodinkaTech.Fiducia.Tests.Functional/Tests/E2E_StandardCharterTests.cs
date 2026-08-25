using FluentAssertions;
using Microsoft.Playwright;
using SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

namespace SamorodinkaTech.Fiducia.Tests.Functional;

/// <summary>
/// Сквозные E2E-тесты для 36 типовых уставов ООО.
/// Каждый тест проходит полный цикл:
/// 1. Сброс БД (без демо-данных)
/// 2. Создание пользователя в LDAP
/// 3. Логин SYS_ADMIN в Admin Console
/// 4. Создание ООО ЮЛ
/// 5. Добавление сотрудника с ролями LE_ADMIN + CEO
/// 6. Выход из Admin Console
/// 7. Логин GD в Board Portal
/// 8. Заполнение полей ЮЛ + выбор типового устава
/// 9. Сохранение и проверка отсутствия ошибок
/// </summary>
public class E2E_StandardCharterTests : BrowserFixture
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    [InlineData(11)]
    [InlineData(12)]
    [InlineData(13)]
    [InlineData(14)]
    [InlineData(15)]
    [InlineData(16)]
    [InlineData(17)]
    [InlineData(18)]
    [InlineData(19)]
    [InlineData(20)]
    [InlineData(21)]
    [InlineData(22)]
    [InlineData(23)]
    [InlineData(24)]
    [InlineData(25)]
    [InlineData(26)]
    [InlineData(27)]
    [InlineData(28)]
    [InlineData(29)]
    [InlineData(30)]
    [InlineData(31)]
    [InlineData(32)]
    [InlineData(33)]
    [InlineData(34)]
    [InlineData(35)]
    [InlineData(36)]
    public async Task StandardCharter_CompleteFlow(int charterNumber)
    {
        // Создаём свежие страницы для каждого теста
        var adminPage = await CreateAdminConsolePageAsync();
        var boardPage = await CreateBoardPortalPageAsync();
        var ldapPage = await CreatePageAsync();

        try
        {
        // ═══════════════════════════════════════════════════════════════════
        // Шаг 0: Удаление всех тестовых пользователей из LDAP
        // ═══════════════════════════════════════════════════════════════════
        await LdapHelper.DeleteAllTestUsersAsync();

        // ═══════════════════════════════════════════════════════════════════
        // Шаг 1: Сброс БД без демо-данных
        // ═══════════════════════════════════════════════════════════════════
        await DbResetHelper.ResetAsync(includeDemo: false, timeout: TimeSpan.FromMinutes(3));

        // ═══════════════════════════════════════════════════════════════════
        // Шаг 2: Создание пользователя в OpenLDAP через phpLDAPadmin
        // ═══════════════════════════════════════════════════════════════════
        await LdapHelper.CreateUserAsync(
            ldapPage,
            CharterTestData.LdapUid,
            CharterTestData.LdapCn,
            CharterTestData.LdapSn,
            CharterTestData.LdapGivenName,
            CharterTestData.LdapPassword,
            addToBoardGroup: true);

        // ═══════════════════════════════════════════════════════════════════
        // Шаг 3: SYS_ADMIN логинится в Admin Console
        // ═══════════════════════════════════════════════════════════════════
        await AuthHelper.LoginAsAdminAsync(adminPage, CharterTestData.SysAdminDisplayName);

        // Verify we're on the main page
        adminPage.Url.Should().Contain("/main");

        // ═══════════════════════════════════════════════════════════════════
        // Шаг 4: Переход в режим Пользователи (access-management)
        // ═══════════════════════════════════════════════════════════════════
        await adminPage.GotoAsync(PortalUrls.GetUrl(Portal.AdminConsole, "/access-management"));
        await AuthHelper.WaitForBlazorReady(adminPage);
        await adminPage.WaitForTimeoutAsync(1000);

        // Verify page loaded
        var pageContent = await adminPage.ContentAsync();
        pageContent.Should().Contain("Сотрудники и доступ");

        // ═══════════════════════════════════════════════════════════════════
        // Шаг 5: Создание ЮЛ типа ООО
        // ═══════════════════════════════════════════════════════════════════
        var leName = CharterTestData.GetLegalEntityName(charterNumber);
        var leInn = CharterTestData.GetLegalEntityInn(charterNumber);

        await AdminConsoleHelper.CreateLegalEntityAsync(adminPage, leName, leInn);

        // ═══════════════════════════════════════════════════════════════════
        // Шаг 6-7: Добавление сотрудника + назначение ролей LE_ADMIN и CEO
        // ═══════════════════════════════════════════════════════════════════
        await AdminConsoleHelper.AssignRolesAsync(
            adminPage,
            CharterTestData.EmployeeLastName,
            CharterTestData.EmployeeFirstName,
            CharterTestData.EmployeeMiddleName,
            CharterTestData.EmployeePosition,
            CharterTestData.EmployeeLogin,
            new[] { CharterTestData.RoleLeAdmin, CharterTestData.RoleCeo });

        // Verify employee appears in the list
        var employeeRow = adminPage.Locator($"td:has-text('{CharterTestData.LdapCn}')");
        (await employeeRow.CountAsync()).Should().BeGreaterThan(0,
            "Сотрудник должен появиться в списке после добавления");

        // ═══════════════════════════════════════════════════════════════════
        // Шаг 8: Администратор системы выходит из Admin Console
        // ═══════════════════════════════════════════════════════════════════
        await AuthHelper.LogoutAsync(adminPage);

        // ═══════════════════════════════════════════════════════════════════
        // Шаг 9: Генеральный директор заходит в Board Portal
        // ═══════════════════════════════════════════════════════════════════
        await AuthHelper.LoginAsBoardUserAsync(
            boardPage,
            CharterTestData.GetBoardPortalDisplayName());

        // Verify we're on the main page
        boardPage.Url.Should().Contain("/main");

        // ═══════════════════════════════════════════════════════════════════
        // Шаг 10: ГД заходит в режим ЮЛ и заполняет все остальные поля
        //         + устанавливает стандартный тип устава №XX
        // ═══════════════════════════════════════════════════════════════════
        var shortName = $"ООО «Тест {charterNumber:D2}»";
        var ogrn = $"1{charterNumber:D2}345678901";

        await BoardPortalHelper.CompleteLegalEntitySetupAsync(
            boardPage,
            charterNumber,
            shortName: shortName,
            ogrn: ogrn);

        // ═══════════════════════════════════════════════════════════════════
        // Шаг 11: Сохранение и проверка отсутствия ошибок
        // ═══════════════════════════════════════════════════════════════════
        // SaveAndVerifyAsync в BoardPortalHelper уже проверяет отсутствие ошибок
        // Дополнительная проверка:
        var hasErrors = await boardPage.EvaluateAsync<bool>(
            "() => document.querySelectorAll('.alert-danger').length > 0");
        hasErrors.Should().BeFalse(
            $"Для типового устава №{charterNumber} не должно быть ошибок при сохранении");
        }
        finally
        {
            await ldapPage.CloseAsync();
            await boardPage.CloseAsync();
            await adminPage.CloseAsync();
        }
    }
}
