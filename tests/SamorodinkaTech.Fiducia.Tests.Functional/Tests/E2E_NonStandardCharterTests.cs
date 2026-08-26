using FluentAssertions;
using Microsoft.Playwright;
using SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

namespace SamorodinkaTech.Fiducia.Tests.Functional;

/// <summary>
/// Сквозные E2E-тесты для нетипового (индивидуального) устава ООО.
/// Каждый тест проходит полный цикл:
/// 1. Сброс БД (без демо-данных)
/// 2. Создание пользователя в LDAP
/// 3. Логин SYS_ADMIN в Admin Console
/// 4. Создание ООО ЮЛ
/// 5. Добавление сотрудника с ролями LE_ADMIN + CEO
/// 6. Выход из Admin Console
/// 7. Логин GD в Board Portal
/// 8. Заполнение полей ЮЛ + выбор нетипового устава
/// 9. Настройка конкретного параметра устава
/// 10. Сохранение и проверка отсутствия ошибок
/// </summary>
public class E2E_NonStandardCharterTests : BrowserFixture
{
    // ══════════════════════════════════════════════════════════════════════
    // Полный цикл: каждый параметр нетипового устава — отдельный тест
    // ══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task NonStandardCharter_ExitAllowed_ShouldSaveWithoutErrors()
    {
        var (adminPage, boardPage, ldapPage) = await SetupFullCycleAsync(1);
        try
        {
            await BoardPortalHelper.SelectNonStandardCharterAsync(boardPage);
            await BoardPortalHelper.AssertNonStandardCharterFieldsVisibleAsync(boardPage);

            await BoardPortalHelper.ConfigureCharterParameterAsync(
                boardPage, NonStandardCharterTestData.ParameterNames.ExitAllowed,
                NonStandardCharterTestData.ExitAllowed);

            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
        }
        finally { await CleanupAsync(adminPage, boardPage, ldapPage); }
    }

    [Fact]
    public async Task NonStandardCharter_ExitMinSharePercent_ShouldSaveWithoutErrors()
    {
        var (adminPage, boardPage, ldapPage) = await SetupFullCycleAsync(2);
        try
        {
            await BoardPortalHelper.SelectNonStandardCharterAsync(boardPage);
            await BoardPortalHelper.ConfigureCharterParameterAsync(
                boardPage, NonStandardCharterTestData.ParameterNames.ExitAllowed,
                NonStandardCharterTestData.ExitAllowed);
            await BoardPortalHelper.ConfigureCharterParameterAsync(
                boardPage, NonStandardCharterTestData.ParameterNames.ExitMinSharePercent,
                NonStandardCharterTestData.ExitMinSharePercent);

            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
        }
        finally { await CleanupAsync(adminPage, boardPage, ldapPage); }
    }

    [Fact]
    public async Task NonStandardCharter_ExitMaxSharePercent_ShouldSaveWithoutErrors()
    {
        var (adminPage, boardPage, ldapPage) = await SetupFullCycleAsync(3);
        try
        {
            await BoardPortalHelper.SelectNonStandardCharterAsync(boardPage);
            await BoardPortalHelper.ConfigureCharterParameterAsync(
                boardPage, NonStandardCharterTestData.ParameterNames.ExitAllowed,
                NonStandardCharterTestData.ExitAllowed);
            await BoardPortalHelper.ConfigureCharterParameterAsync(
                boardPage, NonStandardCharterTestData.ParameterNames.ExitMaxSharePercent,
                NonStandardCharterTestData.ExitMaxSharePercent);

            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
        }
        finally { await CleanupAsync(adminPage, boardPage, ldapPage); }
    }

    [Fact]
    public async Task NonStandardCharter_ExitConditionDescription_ShouldSaveWithoutErrors()
    {
        var (adminPage, boardPage, ldapPage) = await SetupFullCycleAsync(4);
        try
        {
            await BoardPortalHelper.SelectNonStandardCharterAsync(boardPage);
            await BoardPortalHelper.ConfigureCharterParameterAsync(
                boardPage, NonStandardCharterTestData.ParameterNames.ExitAllowed,
                NonStandardCharterTestData.ExitAllowed);
            await BoardPortalHelper.ConfigureCharterParameterAsync(
                boardPage, NonStandardCharterTestData.ParameterNames.ExitConditionDescription,
                NonStandardCharterTestData.ExitConditionDescription);

            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
        }
        finally { await CleanupAsync(adminPage, boardPage, ldapPage); }
    }

    [Fact]
    public async Task NonStandardCharter_ExitRequiresUnanimousOsu_ShouldSaveWithoutErrors()
    {
        var (adminPage, boardPage, ldapPage) = await SetupFullCycleAsync(5);
        try
        {
            await BoardPortalHelper.SelectNonStandardCharterAsync(boardPage);
            await BoardPortalHelper.ConfigureCharterParameterAsync(
                boardPage, NonStandardCharterTestData.ParameterNames.ExitAllowed,
                NonStandardCharterTestData.ExitAllowed);
            await BoardPortalHelper.ConfigureCharterParameterAsync(
                boardPage, NonStandardCharterTestData.ParameterNames.ExitRequiresUnanimousOsu,
                NonStandardCharterTestData.ExitRequiresUnanimousOsu);

            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
        }
        finally { await CleanupAsync(adminPage, boardPage, ldapPage); }
    }

    [Fact]
    public async Task NonStandardCharter_TransferToParticipants_ShouldSaveWithoutErrors()
    {
        var (adminPage, boardPage, ldapPage) = await SetupFullCycleAsync(6);
        try
        {
            await BoardPortalHelper.SelectNonStandardCharterAsync(boardPage);
            await BoardPortalHelper.ConfigureCharterParameterAsync(
                boardPage, NonStandardCharterTestData.ParameterNames.TransferToParticipants,
                NonStandardCharterTestData.TransferToParticipantsWithoutConsent);

            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
        }
        finally { await CleanupAsync(adminPage, boardPage, ldapPage); }
    }

    [Fact]
    public async Task NonStandardCharter_TransferToThirdParties_ShouldSaveWithoutErrors()
    {
        var (adminPage, boardPage, ldapPage) = await SetupFullCycleAsync(7);
        try
        {
            await BoardPortalHelper.SelectNonStandardCharterAsync(boardPage);
            await BoardPortalHelper.ConfigureCharterParameterAsync(
                boardPage, NonStandardCharterTestData.ParameterNames.TransferToThirdParties,
                NonStandardCharterTestData.TransferToThirdParties);

            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
        }
        finally { await CleanupAsync(adminPage, boardPage, ldapPage); }
    }

    [Fact]
    public async Task NonStandardCharter_PreemptiveRight_ShouldSaveWithoutErrors()
    {
        var (adminPage, boardPage, ldapPage) = await SetupFullCycleAsync(8);
        try
        {
            await BoardPortalHelper.SelectNonStandardCharterAsync(boardPage);
            await BoardPortalHelper.ConfigureCharterParameterAsync(
                boardPage, NonStandardCharterTestData.ParameterNames.PreemptiveRight,
                NonStandardCharterTestData.PreemptiveRight);

            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
        }
        finally { await CleanupAsync(adminPage, boardPage, ldapPage); }
    }

    [Fact]
    public async Task NonStandardCharter_InheritanceWithoutConsent_ShouldSaveWithoutErrors()
    {
        var (adminPage, boardPage, ldapPage) = await SetupFullCycleAsync(9);
        try
        {
            await BoardPortalHelper.SelectNonStandardCharterAsync(boardPage);
            await BoardPortalHelper.ConfigureCharterParameterAsync(
                boardPage, NonStandardCharterTestData.ParameterNames.InheritanceWithoutConsent,
                NonStandardCharterTestData.InheritanceWithoutConsent);

            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
        }
        finally { await CleanupAsync(adminPage, boardPage, ldapPage); }
    }

    [Fact]
    public async Task NonStandardCharter_ExecutiveBody_ShouldSaveWithoutErrors()
    {
        var (adminPage, boardPage, ldapPage) = await SetupFullCycleAsync(10);
        try
        {
            await BoardPortalHelper.SelectNonStandardCharterAsync(boardPage);
            await BoardPortalHelper.ConfigureCharterParameterAsync(
                boardPage, NonStandardCharterTestData.ParameterNames.ExecutiveBody,
                NonStandardCharterTestData.ExecutiveBody);

            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
        }
        finally { await CleanupAsync(adminPage, boardPage, ldapPage); }
    }

    [Fact]
    public async Task NonStandardCharter_HasBoardOfDirectors_ShouldSaveAndShowBoardTab()
    {
        var (adminPage, boardPage, ldapPage) = await SetupFullCycleAsync(11);
        try
        {
            await BoardPortalHelper.SelectNonStandardCharterAsync(boardPage);
            await BoardPortalHelper.ConfigureCharterParameterAsync(
                boardPage, NonStandardCharterTestData.ParameterNames.HasBoardOfDirectors,
                NonStandardCharterTestData.HasBoardOfDirectors);

            await BoardPortalHelper.AssertBoardOfDirectorsAvailableAsync(boardPage);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
        }
        finally { await CleanupAsync(adminPage, boardPage, ldapPage); }
    }

    [Fact]
    public async Task NonStandardCharter_BoardDecidesConveningOsu_ShouldSaveWithoutErrors()
    {
        var (adminPage, boardPage, ldapPage) = await SetupFullCycleAsync(12);
        try
        {
            await BoardPortalHelper.SelectNonStandardCharterAsync(boardPage);
            await BoardPortalHelper.ConfigureCharterParameterAsync(
                boardPage, NonStandardCharterTestData.ParameterNames.HasBoardOfDirectors,
                NonStandardCharterTestData.HasBoardOfDirectors);
            await BoardPortalHelper.ConfigureCharterParameterAsync(
                boardPage, NonStandardCharterTestData.ParameterNames.BoardDecidesConveningOsu,
                NonStandardCharterTestData.BoardDecidesConveningOsu);

            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
        }
        finally { await CleanupAsync(adminPage, boardPage, ldapPage); }
    }

    [Fact]
    public async Task NonStandardCharter_VosuThresholdPercent_ShouldSaveWithoutErrors()
    {
        var (adminPage, boardPage, ldapPage) = await SetupFullCycleAsync(13);
        try
        {
            await BoardPortalHelper.SelectNonStandardCharterAsync(boardPage);
            await BoardPortalHelper.ConfigureCharterParameterAsync(
                boardPage, NonStandardCharterTestData.ParameterNames.HasBoardOfDirectors,
                NonStandardCharterTestData.HasBoardOfDirectors);
            await BoardPortalHelper.ConfigureCharterParameterAsync(
                boardPage, NonStandardCharterTestData.ParameterNames.VosuThresholdPercent,
                NonStandardCharterTestData.VosuThresholdPercent);

            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
        }
        finally { await CleanupAsync(adminPage, boardPage, ldapPage); }
    }

    [Fact]
    public async Task NonStandardCharter_AllParameters_ShouldSaveWithoutErrors()
    {
        var (adminPage, boardPage, ldapPage) = await SetupFullCycleAsync(14);
        try
        {
            await BoardPortalHelper.SelectNonStandardCharterAsync(boardPage);
            await BoardPortalHelper.AssertNonStandardCharterFieldsVisibleAsync(boardPage);

            // Настраиваем все параметры нетипового устава
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, NonStandardCharterTestData.ParameterNames.ExitAllowed, NonStandardCharterTestData.ExitAllowed);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, NonStandardCharterTestData.ParameterNames.ExitMinSharePercent, NonStandardCharterTestData.ExitMinSharePercent);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, NonStandardCharterTestData.ParameterNames.ExitMaxSharePercent, NonStandardCharterTestData.ExitMaxSharePercent);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, NonStandardCharterTestData.ParameterNames.ExitConditionDescription, NonStandardCharterTestData.ExitConditionDescription);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, NonStandardCharterTestData.ParameterNames.ExitRequiresUnanimousOsu, NonStandardCharterTestData.ExitRequiresUnanimousOsu);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, NonStandardCharterTestData.ParameterNames.TransferToParticipants, NonStandardCharterTestData.TransferToParticipantsWithoutConsent);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, NonStandardCharterTestData.ParameterNames.TransferToThirdParties, NonStandardCharterTestData.TransferToThirdParties);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, NonStandardCharterTestData.ParameterNames.PreemptiveRight, NonStandardCharterTestData.PreemptiveRight);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, NonStandardCharterTestData.ParameterNames.InheritanceWithoutConsent, NonStandardCharterTestData.InheritanceWithoutConsent);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, NonStandardCharterTestData.ParameterNames.ExecutiveBody, NonStandardCharterTestData.ExecutiveBody);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, NonStandardCharterTestData.ParameterNames.HasBoardOfDirectors, NonStandardCharterTestData.HasBoardOfDirectors);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, NonStandardCharterTestData.ParameterNames.BoardDecidesConveningOsu, NonStandardCharterTestData.BoardDecidesConveningOsu);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, NonStandardCharterTestData.ParameterNames.VosuThresholdPercent, NonStandardCharterTestData.VosuThresholdPercent);

            await BoardPortalHelper.AssertBoardOfDirectorsAvailableAsync(boardPage);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
        }
        finally { await CleanupAsync(adminPage, boardPage, ldapPage); }
    }

    // ══════════════════════════════════════════════════════════════════════
    // Общие методы полного цикла
    // ══════════════════════════════════════════════════════════════════════

    private async Task<(IPage adminPage, IPage boardPage, IPage ldapPage)> SetupFullCycleAsync(int testIndex)
    {
        var adminPage = await CreateAdminConsolePageAsync();
        var boardPage = await CreateBoardPortalPageAsync();
        var ldapPage = await CreatePageAsync();

        // Шаг 0: Удаление тестовых пользователей LDAP
        await LdapHelper.DeleteAllTestUsersAsync();

        // Шаг 1: Сброс БД
        await DbResetHelper.ResetAsync(includeDemo: false, timeout: TimeSpan.FromMinutes(3));

        // Шаг 2: Создание пользователя в LDAP
        await LdapHelper.CreateUserAsync(
            ldapPage,
            NonStandardCharterTestData.LdapUid,
            NonStandardCharterTestData.LdapCn,
            NonStandardCharterTestData.LdapSn,
            NonStandardCharterTestData.LdapGivenName,
            NonStandardCharterTestData.LdapPassword,
            addToBoardGroup: true);

        // Шаг 3: SYS_ADMIN логинится в Admin Console
        await AuthHelper.LoginAsAdminAsync(adminPage, NonStandardCharterTestData.SysAdminDisplayName);
        adminPage.Url.Should().Contain("/main");

        // Шаг 4: Переход в режим Пользователи
        await adminPage.GotoAsync(PortalUrls.GetUrl(Portal.AdminConsole, "/access-management"));
        await AuthHelper.WaitForBlazorReady(adminPage);
        await adminPage.WaitForTimeoutAsync(1000);

        // Шаг 5: Создание ООО ЮЛ
        var leName = NonStandardCharterTestData.GetLegalEntityName(testIndex);
        var leInn = NonStandardCharterTestData.GetLegalEntityInn(testIndex);
        await AdminConsoleHelper.CreateLegalEntityAsync(adminPage, leName, leInn);

        // Шаг 6-7: Добавление сотрудника + роли
        await AdminConsoleHelper.AssignRolesAsync(
            adminPage,
            NonStandardCharterTestData.EmployeeLastName,
            NonStandardCharterTestData.EmployeeFirstName,
            NonStandardCharterTestData.EmployeeMiddleName,
            NonStandardCharterTestData.EmployeePosition,
            NonStandardCharterTestData.EmployeeLogin,
            new[] { NonStandardCharterTestData.RoleLeAdmin, NonStandardCharterTestData.RoleCeo });

        // Шаг 8: Администратор выходит
        await AuthHelper.LogoutAsync(adminPage);

        // Шаг 9: ГД заходит в Board Portal
        await AuthHelper.LoginAsBoardUserAsync(boardPage, NonStandardCharterTestData.LdapCn);
        boardPage.Url.Should().Contain("/main");

        // Шаг 10: Заполнение полей ЮЛ
        var shortName = NonStandardCharterTestData.GetShortName(testIndex);
        var ogrn = NonStandardCharterTestData.GetOgrn(testIndex);
        await BoardPortalHelper.FillLegalEntityFieldsAsync(boardPage, shortName, ogrn);

        return (adminPage, boardPage, ldapPage);
    }

    private static async Task CleanupAsync(IPage adminPage, IPage boardPage, IPage ldapPage)
    {
        await ldapPage.CloseAsync();
        await boardPage.CloseAsync();
        await adminPage.CloseAsync();
    }
}
