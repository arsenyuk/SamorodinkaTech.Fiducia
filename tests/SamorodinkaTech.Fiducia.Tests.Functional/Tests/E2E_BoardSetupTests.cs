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

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(64);
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

            // Проверяем, что страница загрузилась (ищем h3 — Blazor рендерит в DOM до SignalR)
            var h3 = await boardPage.WaitForSelectorAsync("h3", new() { Timeout = 15000 });
            h3.Should().NotBeNull("h3 заголовок wizard'а должен присутствовать");

            // Проверяем структуру СД
            await boardPage.WaitForSelectorAsync("text=Председатель СД");
            await boardPage.WaitForSelectorAsync("text=Зам. председателя");
            await boardPage.WaitForSelectorAsync("text=Секретарь СД");

            // Добавляем Председателя СД
            await boardPage.ClickAsync("text=Добавить участника СД");
            await boardPage.WaitForSelectorAsync(".modal.show");

            // Выбираем роль "Председатель СД"
            await boardPage.SelectOptionAsync(".modal select", "CHAIR");

            // Вводим ФИО
            var chairName = "Иванов Иван Иванович";
            await boardPage.FillAsync(".modal input[type='text']", chairName);

            // Сохраняем
            await boardPage.ClickAsync(".modal button.btn-primary");
            await boardPage.WaitForSelectorAsync(".modal.show", new() { State = WaitForSelectorState.Detached });

            // Проверяем, что Председатель появился в таблице
            await boardPage.WaitForSelectorAsync($"text={chairName}");

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
            await CleanupAsync(adminPage, boardPage, ldapPage);
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

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(65);
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
            await AddBoardMemberAsync(boardPage, "CHAIR", "Петров Пётр Петрович");

            // Добавляем Зам. председателя СД
            await AddBoardMemberAsync(boardPage, "DEPUTY_CHAIR", "Сидоров Сидор Сидорович");

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
            await CleanupAsync(adminPage, boardPage, ldapPage);
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

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(66);
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
            await AddBoardMemberAsync(boardPage, "CHAIR", "Козлов Козлом Козлович");

            // Добавляем Секретаря СД
            await AddBoardMemberAsync(boardPage, "SECRETARY", "Федорова Федора Федоровна");

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
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    // ══════════════════════════════════════════════════════════════════════
    // Вспомогательные методы
    // ══════════════════════════════════════════════════════════════════════

    private async Task NavigateToBoardSetupAsync(IPage boardPage)
    {
        var url = PortalUrls.GetUrl(Portal.BoardPortal, "/board-setup");
        await boardPage.GotoAsync(url);
        await AuthHelper.WaitForBlazorReady(boardPage);
        await boardPage.WaitForTimeoutAsync(3000);
    }

    private async Task AddBoardMemberAsync(IPage boardPage, string roleCode, string fullName)
    {
        await boardPage.ClickAsync("text=Добавить участника СД");
        await boardPage.WaitForSelectorAsync(".modal.show");

        // Выбираем роль
        await boardPage.SelectOptionAsync(".modal select", roleCode);

        // Вводим ФИО
        await boardPage.FillAsync(".modal .mb-3 input[type='text']", fullName);

        // Сохраняем
        await boardPage.ClickAsync(".modal button.btn-primary");
        await boardPage.WaitForSelectorAsync(".modal.show", new() { State = WaitForSelectorState.Detached });

        // Проверяем, что участник появился в таблице
        await boardPage.WaitForSelectorAsync($"text={fullName}");
    }

    private async Task<(IPage adminPage, IPage boardPage, IPage ldapPage, string login)> SetupFullCycleAsync(int entityIndex)
    {
        await InfrastructureHelper.EnsureInfrastructureReadyAsync();

        var adminPage = await CreateAdminConsolePageAsync();
        var boardPage = await CreateBoardPortalPageAsync();
        var ldapPage = await CreatePageAsync();

        await CharterTestGlobalInit.InitializeAsync(adminPage, ldapPage);
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

        return (adminPage, boardPage, ldapPage, gdLogin);
    }

    private static async Task AddParticipantsAsync(IPage boardPage, int entityIndex)
    {
        var persons = CharterTestDataFixed.PersonsByEntity[entityIndex];

        foreach (var p in persons.Participants)
        {
            await BoardPortalHelper.AddParticipantAsync(
                boardPage,
                p.FullName,
                sharePercent: p.SharePercent);
        }

        await BoardPortalHelper.AssertParticipantCountAsync(
            boardPage,
            persons.Participants.Count);
    }

    private static async Task CleanupAsync(IPage adminPage, IPage boardPage, IPage ldapPage)
    {
        await ldapPage.CloseAsync();
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
