using FluentAssertions;
using Microsoft.Playwright;
using SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

namespace SamorodinkaTech.Fiducia.Tests.Functional;

/// <summary>
/// Сквозные E2E-тесты для вкладки «ГД» (генеральный директор) на странице ЮЛ.
/// Проверяет назначение участника генеральным директором, ввод СНИЛС,
/// видимость вкладки в зависимости от типа устава и исполнительного органа.
/// БД сбрасывается ОДИН раз перед прогоном всех тестов.
/// Запрещено параллельное исполнение (Collection "CharterTests").
/// Документация: docs/e2e-general-director.md
/// </summary>
[Collection("CharterTests")]
public class E2E_GeneralDirectorTests : BrowserFixture
{
    public E2E_GeneralDirectorTests(GlobalFixture globalFixture) : base(globalFixture)
    {
    }

    // ══════════════════════════════════════════════════════════════════════
    // Тесты
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Тест 58: Назначение ГД + информирование об увольнении (ст. 280 ТК РФ).
    /// Сценарий: индивидуальный устав → ExecBody=A → добавление участников →
    /// назначение ГД → сохранение → переход на /ceo-resignation →
    /// заполнение формы → отправка уведомления участникам → проверка результата.
    /// </summary>
    [Fact]
    public async Task GeneralDirector_TabVisible_ShouldAssignParticipantAndSave()
    {
        SkipIfPreviousFailed();

        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "GeneralDirector_TabVisible";

        var (adminPage, boardPage, login) = await SetupFullCycleAsync(58);
        try
        {
            // ── Часть 1: Назначение ГД ────────────────────────────
            // Выбираем индивидуальный устав
            await BoardPortalHelper.SelectCustomCharterAsync(boardPage);
            // ExecutiveBody = A (по умолчанию для индивидуального устава)
            await BoardPortalHelper.SetExecutiveBodyAsync(boardPage, "A");

            // Добавляем участника
            await AddParticipantsAsync(boardPage, 58);

            // Проверяем, что вкладка «ГД» видна
            await BoardPortalHelper.AssertGeneralDirectorTabVisibleAsync(boardPage);

            // Переходим на вкладку «ГД»
            await BoardPortalHelper.ClickGeneralDirectorTabAsync(boardPage);

            // Выбираем участника в качестве ГД
            var persons = CharterTestDataFixed.PersonsByEntity[58];
            var participantName = persons.Participants[0].FullName;
            await BoardPortalHelper.SelectGeneralDirectorAsync(boardPage, participantName);

            // Проверяем, что данные участника отображаются
            await BoardPortalHelper.AssertGeneralDirectorDataVisibleAsync(boardPage, participantName);
            await BoardPortalHelper.AssertGeneralDirectorReadonlyFieldsAsync(boardPage);

            // Устанавливаем СНИЛС
            await BoardPortalHelper.SetGeneralDirectorSnilsAsync(boardPage, "123-456-789 00");

            // Сохраняем
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);

            // ── Часть 2: ГД информирует об увольнении (ст. 280 ТК РФ) ──
            // Повторный логин как ГД (после сохранения CEO роль назначена через ЕДИН binding)
            var gdLogin = persons.Gd?.Login ?? persons.Participants[0].Login;
            await AuthHelper.LoginAsBoardUserAsync(boardPage, gdLogin);
            boardPage.Url.Should().Contain("/main");

            // Навигация на страницу уведомления об увольнении
            await boardPage.GotoAsync("http://localhost:5002/ceo-resignation");
            await AuthHelper.WaitForBlazorReady(boardPage);

            // Проверка загрузки страницы
            await boardPage.WaitForSelectorAsync(
                "text=Уведомление участников об увольнении Генерального директора",
                new() { Timeout = DefaultTimeout });

            // Проверка preview-данных
            var pageContent = await boardPage.ContentAsync();
            pageContent.Should().Contain("Статья 280 ТК РФ");
            pageContent.Should().Contain("Участников:");

            // Дата увольнения (через 45 дней — ст. 280 ТК РФ требует ≥ 30 дней)
            var dateInput = await boardPage.WaitForSelectorAsync(
                "input[type='date']",
                new() { Timeout = DefaultTimeout });
            var resignationDate = DateOnly.FromDateTime(DateTime.Today.AddDays(45));
            // FillAsync корректно триггерит Blazor binding (как в VosuDemandTests)
            await dateInput!.FillAsync(resignationDate.ToString("yyyy-MM-dd"));

            // Проверка что textarea повестки существует и содержит данные
            var agendaTextarea = await boardPage.WaitForSelectorAsync(
                "textarea",
                new() { Timeout = DefaultTimeout });
            agendaTextarea.Should().NotBeNull("повестка ВОСУ должна быть");

            // Место ознакомления
            var reviewLocationInput = await boardPage.QuerySelectorAsync("input[placeholder*='Пятницкая']");
            if (reviewLocationInput is not null)
            {
                await reviewLocationInput.FillAsync("г. Москва, ул. Тверская, д. 1, в рабочие дни с 10:00 до 16:00");
            }

            // Отправка уведомления
            await boardPage.ClickAsync("button:text('Сформировать и отправить уведомление')");

            // Проверка результата
            await boardPage.WaitForSelectorAsync(
                "text=Уведомление отправлено",
                new() { Timeout = DefaultTimeout });

            var resultContent = await boardPage.ContentAsync();
            resultContent.Should().Contain("участников");
            resultContent.Should().Contain("DOCX-файлов");

            // ── Финал: проверка страниц и аудита ──────────────────
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);
            await AssertAuditForGeneralDirectorAsync(login);
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
    /// Тест 59: Назначение ГД из двух участников — выбор второго участника.
    /// </summary>
    [Fact]
    public async Task GeneralDirector_TwoParticipants_ShouldSelectSecond()
    {
        SkipIfPreviousFailed();

        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "GeneralDirector_TwoParticipants";

        var (adminPage, boardPage, login) = await SetupFullCycleAsync(59);
        try
        {
            await BoardPortalHelper.SelectCustomCharterAsync(boardPage);
            await BoardPortalHelper.SetExecutiveBodyAsync(boardPage, "A");
            await AddParticipantsAsync(boardPage, 59);

            await BoardPortalHelper.AssertGeneralDirectorTabVisibleAsync(boardPage);
            await BoardPortalHelper.ClickGeneralDirectorTabAsync(boardPage);

            // Выбираем второго участника
            var persons = CharterTestDataFixed.PersonsByEntity[59];
            var secondParticipantName = persons.Participants[1].FullName;
            await BoardPortalHelper.SelectGeneralDirectorAsync(boardPage, secondParticipantName);

            await BoardPortalHelper.AssertGeneralDirectorDataVisibleAsync(boardPage, secondParticipantName);

            // Устанавливаем СНИЛС
            await BoardPortalHelper.SetGeneralDirectorSnilsAsync(boardPage, "987-654-321 00");

            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);
            await AssertAuditForGeneralDirectorAsync(login);
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
    /// Тест 60: Сохранение ГД с СНИЛС и проверка сохранения данных.
    /// </summary>
    [Fact]
    public async Task GeneralDirector_SaveWithSnils_ShouldPersistData()
    {
        SkipIfPreviousFailed();

        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "GeneralDirector_SaveWithSnils";

        var (adminPage, boardPage, login) = await SetupFullCycleAsync(60);
        try
        {
            await BoardPortalHelper.SelectCustomCharterAsync(boardPage);
            await BoardPortalHelper.SetExecutiveBodyAsync(boardPage, "A");
            await AddParticipantsAsync(boardPage, 60);

            await BoardPortalHelper.ClickGeneralDirectorTabAsync(boardPage);

            var persons = CharterTestDataFixed.PersonsByEntity[60];
            var participantName = persons.Participants[0].FullName;
            await BoardPortalHelper.SelectGeneralDirectorAsync(boardPage, participantName);
            await BoardPortalHelper.SetGeneralDirectorSnilsAsync(boardPage, "111-222-333 44");

            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);

            // Проверяем, что после перезагрузки страницы данные сохранились
            // (переходим на другую вкладку и обратно)
            await BoardPortalHelper.ClickGeneralDirectorTabAsync(boardPage);
            await BoardPortalHelper.AssertGeneralDirectorDataVisibleAsync(boardPage, participantName);

            await VerifyPagesAsync(boardPage, adminPage, testStartTime);
            await AssertAuditForGeneralDirectorAsync(login);
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
    /// Тест 61: Вкладка «ГД» НЕ отображается при ExecutiveBody=B (каждый участник — директор).
    /// </summary>
    [Fact]
    public async Task GeneralDirector_ExecBodyB_ShouldHideTab()
    {
        SkipIfPreviousFailed();

        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "GeneralDirector_ExecBodyB";

        var (adminPage, boardPage, login) = await SetupFullCycleAsync(61);
        try
        {
            await BoardPortalHelper.SelectCustomCharterAsync(boardPage);
            await BoardPortalHelper.SetExecutiveBodyAsync(boardPage, "B");
            await AddParticipantsAsync(boardPage, 61);

            // Вкладка «ГД» НЕ должна отображаться при ExecutiveBody=B
            await BoardPortalHelper.AssertGeneralDirectorTabNotVisibleAsync(boardPage);

            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);
            await AssertAuditForGeneralDirectorAsync(login);
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
    /// Тест 62: Вкладка «ГД» НЕ отображается при ExecutiveBody=C (все совместно).
    /// </summary>
    [Fact]
    public async Task GeneralDirector_ExecBodyC_ShouldHideTab()
    {
        SkipIfPreviousFailed();

        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "GeneralDirector_ExecBodyC";

        var (adminPage, boardPage, login) = await SetupFullCycleAsync(62);
        try
        {
            await BoardPortalHelper.SelectCustomCharterAsync(boardPage);
            await BoardPortalHelper.SetExecutiveBodyAsync(boardPage, "C");
            await AddParticipantsAsync(boardPage, 62);

            // Вкладка «ГД» НЕ должна отображаться при ExecutiveBody=C
            await BoardPortalHelper.AssertGeneralDirectorTabNotVisibleAsync(boardPage);

            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);
            await AssertAuditForGeneralDirectorAsync(login);
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
    /// Тест 63: Вкладка «ГД» отображается для типового устава с ExecutiveBody=A.
    /// </summary>
    [Fact]
    public async Task GeneralDirector_StandardCharter_ShouldShowTab()
    {
        SkipIfPreviousFailed();

        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "GeneralDirector_StandardCharter_ExecBodyA";

        var (adminPage, boardPage, login) = await SetupFullCycleAsync(63);
        try
        {
            // Типовой устав № 1 (ExecutiveBody=A по умолчанию)
            await BoardPortalHelper.SelectStandardCharterAsync(boardPage, 1);

            await AddParticipantsAsync(boardPage, 63);

            // Вкладка «ГД» должна отображаться для типового устава с ExecutiveBody=A
            await BoardPortalHelper.AssertGeneralDirectorTabVisibleAsync(boardPage);

            await BoardPortalHelper.ClickGeneralDirectorTabAsync(boardPage);

            var persons = CharterTestDataFixed.PersonsByEntity[63];
            var participantName = persons.Participants[0].FullName;
            await BoardPortalHelper.SelectGeneralDirectorAsync(boardPage, participantName);
            await BoardPortalHelper.SetGeneralDirectorSnilsAsync(boardPage, "555-666-777 88");

            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);
            await AssertAuditForGeneralDirectorAsync(login);
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

    private static async Task VerifyPagesAsync(IPage boardPage, IPage adminPage, DateTimeOffset testStartTime)
    {
        await PageVerificationHelper.VerifyBoardPortalPagesAsync(boardPage, testStartTime);
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
                dulTypeCode: p.DulTypeCode,
                dulSeries: p.DulSeries,
                dulNumber: p.DulNumber);
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

    private static async Task AssertAuditForGeneralDirectorAsync(string login)
    {
        await AuditLogHelper.AssertLoginLoggedAsync(login);
        await AuditLogHelper.AssertDataUpdateLoggedAsync("legal-entities");
    }
}
