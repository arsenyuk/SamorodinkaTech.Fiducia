using FluentAssertions;
using Microsoft.Playwright;
using SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

namespace SamorodinkaTech.Fiducia.Tests.Functional;

/// <summary>
/// Сквозные E2E-тесты для первичного ввода состава Совета директоров.
/// Тест проверяет wizard назначения ролей в СД для ООО с нетиповым уставом.
/// БД сбрасывается ОДИН раз перед прогоном всех тестов.
/// Тестовый сценарий:
/// 1. Логин ГД в Board Portal
/// 2. Настройка ЮЛ: нетиповый устав + Совет директоров
/// 3. Добавление участников общества
/// 4. Переход на страницу первичного ввода состава СД
/// 5. Добавление Председателя СД
/// 6. Добавление Зам. председателя (Вариант 2)
/// 7. Сохранение и проверка отсутствия ошибок
/// 8. Проверка страницы Admin Console
/// Документация: docs/e2e-board-setup.md
/// </summary>
[Collection("CharterTests")]
public class E2E_BoardSetupTests : BrowserFixture
{
    public E2E_BoardSetupTests(GlobalFixture globalFixture) : base(globalFixture)
    {
    }

    // ══════════════════════════════════════════════════════════════════════
    // Тесты
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Вариант 1: Только Председатель СД — wizard открывается, добавление Chairman, сохранение.
    /// </summary>
    [Fact]
    public async Task BoardSetup_Variant1_ChairOnly_ShouldSaveWithoutErrors()
    {
        SkipIfPreviousFailed();

        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "BoardSetup_Variant1_ChairOnly";

        var (adminPage, boardPage, login) = await SetupFullCycleAsync(64);
        try
        {
            // Настраиваем ЮЛ: нетиповый устав + Совет директоров
            await BoardPortalHelper.SelectNonStandardCharterAsync(boardPage);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "has-board", "true");

            // Добавляем участников общества
            await AddParticipantsAsync(boardPage, 64);

            // Сохраняем ЮЛ
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);

            // Переходим на страницу первичного ввода состава СД
            await NavigateToBoardSetupAsync(boardPage);

            // Проверяем, что страница загрузилась
            await boardPage.WaitForSelectorAsync("text=Совет директоров — настройка состава", new() { Timeout = DefaultTimeout });

            // Добавляем Председателя СД
            await AddBoardMemberAsync(boardPage, "CHAIR", "Петров", "Пётр", "Петрович");

            // Сохраняем состав СД
            await boardPage.ClickAsync("text=Сохранить состав СД");
            await boardPage.WaitForSelectorAsync("text=Состав Совета директоров успешно сохранён");

            // Проверяем страницу Admin Console
            await VerifyAdminConsolePageAsync(adminPage, testStartTime);
        }
        catch
        {
            GlobalFixture.MarkFailed();
            throw;
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage);
        }
    }

    /// <summary>
    /// Вариант 2: Председатель + Зам. председателя — wizard с двумя ролями.
    /// </summary>
    [Fact]
    public async Task BoardSetup_Variant2_ChairAndDeputy_ShouldSaveWithoutErrors()
    {
        SkipIfPreviousFailed();

        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "BoardSetup_Variant2_ChairAndDeputy";

        var (adminPage, boardPage, login) = await SetupFullCycleAsync(65);
        try
        {
            // Настраиваем ЮЛ: нетиповый устав + Совет директоров
            await BoardPortalHelper.SelectNonStandardCharterAsync(boardPage);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "has-board", "true");

            // Добавляем участников общества
            await AddParticipantsAsync(boardPage, 65);

            // Сохраняем ЮЛ
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);

            // Переходим на страницу первичного ввода состава СД
            await NavigateToBoardSetupAsync(boardPage);

            // Проверяем, что страница загрузилась
            await boardPage.WaitForSelectorAsync("text=Совет директоров — настройка состава");

            // Добавляем Председателя СД
            await AddBoardMemberAsync(boardPage, "CHAIR", "Петров", "Пётр", "Петрович");

            // Добавляем Зам. председателя СД
            await AddBoardMemberAsync(boardPage, "DEPUTY_CHAIR", "Сидоров", "Сидор", "Сидорович");

            // Сохраняем состав СД
            await boardPage.ClickAsync("text=Сохранить состав СД");
            await boardPage.WaitForSelectorAsync("text=Состав Совета директоров успешно сохранён");

            // Проверяем страницу Admin Console
            await VerifyAdminConsolePageAsync(adminPage, testStartTime);
        }
        catch
        {
            GlobalFixture.MarkFailed();
            throw;
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage);
        }
    }

    /// <summary>
    /// Вариант 3: Председатель + Секретарь — wizard с двумя ролями.
    /// </summary>
    [Fact]
    public async Task BoardSetup_Variant3_ChairAndSecretary_ShouldSaveWithoutErrors()
    {
        SkipIfPreviousFailed();

        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "BoardSetup_Variant3_ChairAndSecretary";

        var (adminPage, boardPage, login) = await SetupFullCycleAsync(66);
        try
        {
            // Настраиваем ЮЛ: нетиповый устав + Совет директоров
            await BoardPortalHelper.SelectNonStandardCharterAsync(boardPage);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "has-board", "true");

            // Добавляем участников общества
            await AddParticipantsAsync(boardPage, 66);

            // Сохраняем ЮЛ
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);

            // Переходим на страницу первичного ввода состава СД
            await NavigateToBoardSetupAsync(boardPage);

            // Проверяем, что страница загрузилась
            await boardPage.WaitForSelectorAsync("text=Совет директоров — настройка состава");

            // Добавляем Председателя СД
            await AddBoardMemberAsync(boardPage, "CHAIR", "Козлов", "Козлом", "Козлович");

            // Добавляем Секретаря СД
            await AddBoardMemberAsync(boardPage, "SECRETARY", "Федорова", "Федора", "Федоровна");

            // Сохраняем состав СД
            await boardPage.ClickAsync("text=Сохранить состав СД");
            await boardPage.WaitForSelectorAsync("text=Состав Совета директоров успешно сохранён");

            // Проверяем страницу Admin Console
            await VerifyAdminConsolePageAsync(adminPage, testStartTime);
        }
        catch
        {
            GlobalFixture.MarkFailed();
            throw;
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage);
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // Вспомогательные методы
    // ══════════════════════════════════════════════════════════════════════

    private async Task NavigateToBoardSetupAsync(IPage boardPage)
    {
        // ЮЛ определяется из текущего пользователя (LegalEntityHelper)
        var url = PortalUrls.GetUrl(Portal.BoardPortal, "/board-setup");
        await boardPage.GotoAsync(url);
        await AuthHelper.WaitForBlazorReady(boardPage);
        await boardPage.WaitForTimeoutAsync(3000);
    }

    private async Task AddBoardMemberAsync(IPage boardPage, string roleCode, string lastName, string firstName, string? middleName = null)
    {
        await boardPage.ClickAsync("text=Добавить участника СД");
        await boardPage.WaitForSelectorAsync(".modal.show");

        // Выбираем роль — первый select в модалке
        var roleSelect = boardPage.Locator(".modal select").First;
        if (await roleSelect.CountAsync() == 0)
            throw new InvalidOperationException("Не найден select роли в модалке «Добавить участника СД»");
        await roleSelect.SelectOptionAsync(roleCode);

        // Вводим ФИО по data-testid + DispatchEvent для @bind
        var lastNameInput = boardPage.GetByTestId("bm-lastName");
        if (await lastNameInput.CountAsync() == 0)
            throw new InvalidOperationException("Не найдено поле «Фамилия» (data-testid=bm-lastName) в модалке");
        await lastNameInput.FillAsync(lastName);
        await lastNameInput.DispatchEventAsync("change");

        var firstNameInput = boardPage.GetByTestId("bm-firstName");
        if (await firstNameInput.CountAsync() == 0)
            throw new InvalidOperationException("Не найдено поле «Имя» (data-testid=bm-firstName) в модалке");
        await firstNameInput.FillAsync(firstName);
        await firstNameInput.DispatchEventAsync("change");

        if (!string.IsNullOrEmpty(middleName))
        {
            var middleNameInput = boardPage.GetByTestId("bm-middleName");
            if (await middleNameInput.CountAsync() == 0)
                throw new InvalidOperationException("Не найдено поле «Отчество» (data-testid=bm-middleName) в модалке");
            await middleNameInput.FillAsync(middleName);
            await middleNameInput.DispatchEventAsync("change");
        }

        // Сохраняем
        var saveBtn = boardPage.Locator(".modal button.btn-primary");
        if (await saveBtn.CountAsync() == 0)
            throw new InvalidOperationException("Не найдена кнопка «Добавить» в модалке");
        await saveBtn.ClickAsync();
        await boardPage.WaitForSelectorAsync(".modal.show", new() { State = WaitForSelectorState.Detached });

        // Проверяем, что участник появился в таблице
        var fullName = string.IsNullOrEmpty(middleName) ? $"{lastName} {firstName}" : $"{lastName} {firstName} {middleName}";
        await boardPage.WaitForSelectorAsync($"text={fullName}");
    }

    private async Task<(IPage adminPage, IPage boardPage, string login)> SetupFullCycleAsync(int entityIndex)
    {
        await InfrastructureHelper.EnsureInfrastructureReadyAsync();

        var adminPage = await CreateAdminConsolePageAsync();
        var boardPage = await CreateBoardPortalPageAsync();

        await CharterTestGlobalInit.InitializeAsync();
        await CharterTestSeeder.EnsureSeededAsync(adminPage, entityIndex);

        var entity = CharterTestDataFixed.LegalEntities[entityIndex - 1];
        var persons = CharterTestDataFixed.PersonsByEntity[entityIndex];

        var gdLogin = persons.Gd?.Login ?? persons.Participants[0].Login;
        await AuthHelper.LoginAsBoardUserAsync(boardPage, gdLogin);
        boardPage.Url.Should().Contain("/main");

        await BoardPortalHelper.FillLegalEntityFieldsAsync(
            boardPage,
            shortName: entity.ShortName,
            ogrn: entity.Ogrn);

        return (adminPage, boardPage, gdLogin);
    }

    private static async Task AddParticipantsAsync(IPage boardPage, int entityIndex)
    {
        var persons = CharterTestDataFixed.PersonsByEntity[entityIndex];

        foreach (var p in persons.Participants)
        {
            await BoardPortalHelper.AddParticipantAsync(
                boardPage,
                p.LastName, p.FirstName, p.MiddleName,
                sharePercent: p.SharePercent,
                paymentInfo: "Оплачено в полном объёме");
        }

        await BoardPortalHelper.AssertParticipantCountAsync(
            boardPage,
            persons.Participants.Count);
    }

    private static async Task CleanupAsync(IPage adminPage, IPage boardPage)
    {
        await boardPage.CloseAsync();
        await adminPage.CloseAsync();
    }

    private static async Task VerifyAdminConsolePageAsync(IPage adminPage, DateTimeOffset testStartTime)
    {
        // Переходим на страницу назначений ролей в Admin Console
        await adminPage.GotoAsync(PortalUrls.GetUrl(Portal.AdminConsole, "/board-participant-roles"));
        await AuthHelper.WaitForBlazorReady(adminPage);

        // Проверяем, что страница загрузилась
        var title = await adminPage.TextContentAsync("h3");
        title.Should().Contain("Назначения ролей в СД");
    }
}
