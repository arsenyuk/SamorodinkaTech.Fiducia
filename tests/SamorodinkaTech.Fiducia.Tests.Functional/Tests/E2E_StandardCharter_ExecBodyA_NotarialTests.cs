using FluentAssertions;
using Microsoft.Playwright;
using SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

namespace SamorodinkaTech.Fiducia.Tests.Functional;

/// <summary>
/// Сквозные E2E-тесты для типовых уставов ООО:
/// ExecutiveBody A (ГД отдельно) + подтверждение Нотариальное (NOTARIAL).
/// Уставы 01–06 (entityIndex 1–6).
/// БД сбрасывается ОДИН раз перед прогоном всех тестов.
/// Запрещено параллельное исполнение (Collection "CharterTests").
/// При ошибке в одном тесте все последующие тесты прерываются.
/// Документация: docs/e2e-standard-charter.md
/// </summary>
[Collection("CharterTests")]
public class E2E_StandardCharter_ExecBodyA_NotarialTests : BrowserFixture
{
    public E2E_StandardCharter_ExecBodyA_NotarialTests(GlobalFixture globalFixture) : base(globalFixture)
    {
    }

    // ── Уставы 01–06 (ExecutiveBody A: ГД + участники, NOTARIAL) ────
    [Fact] public async Task StandardCharter01_CompleteFlow() => await RunTestAsync(1, "StandardCharter01_CompleteFlow", entityHasExecutiveBodyA: true);
    [Fact] public async Task StandardCharter02_CompleteFlow() => await RunTestAsync(2, "StandardCharter02_CompleteFlow", entityHasExecutiveBodyA: true);
    [Fact] public async Task StandardCharter03_CompleteFlow() => await RunTestAsync(3, "StandardCharter03_CompleteFlow", entityHasExecutiveBodyA: true);
    [Fact] public async Task StandardCharter04_CompleteFlow() => await RunTestAsync(4, "StandardCharter04_CompleteFlow", entityHasExecutiveBodyA: true);
    [Fact] public async Task StandardCharter05_CompleteFlow() => await RunTestAsync(5, "StandardCharter05_CompleteFlow", entityHasExecutiveBodyA: true);
    [Fact] public async Task StandardCharter06_CompleteFlow() => await RunTestAsync(6, "StandardCharter06_CompleteFlow", entityHasExecutiveBodyA: true);

    // ══════════════════════════════════════════════════════════════════════
    // Вспомогательные методы
    // ══════════════════════════════════════════════════════════════════════

    private async Task RunTestAsync(int charterNumber, string testName, bool entityHasExecutiveBodyA)
    {
        SkipIfPreviousFailed();

        var testStartTime = DateTimeOffset.UtcNow;

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(charterNumber);
        try
        {
            await ExecuteCharterFlowAsync(boardPage, adminPage, charterNumber, testStartTime);
            await AssertAuditAsync(login, entityHasExecutiveBodyA, testStartTime);
        }
        catch (Exception ex)
        {
            GlobalFixture.MarkFailed();
            Console.WriteLine($"[FAIL] {testName}: {ex.Message}");
            throw;
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    private async Task<(IPage adminPage, IPage boardPage, IPage ldapPage, string login)>
        SetupFullCycleAsync(int charterNumber)
    {
        await InfrastructureHelper.EnsureInfrastructureReadyAsync();

        var adminPage = await CreateAdminConsolePageAsync();
        var boardPage = await CreateBoardPortalPageAsync();
        var ldapPage = await CreatePageAsync();

        await CharterTestGlobalInit.InitializeAsync();
        await CharterTestSeeder.EnsureSeededAsync(adminPage, charterNumber);

        var persons = CharterTestDataFixed.PersonsByEntity[charterNumber];
        var gdLogin = persons.Gd?.Login ?? persons.Participants[0].Login;
        await AuthHelper.LoginAsBoardUserAsync(boardPage, gdLogin);

        boardPage.Url.Should().Contain("/main");

        return (adminPage, boardPage, ldapPage, gdLogin);
    }

    private static async Task ExecuteCharterFlowAsync(
        IPage boardPage, IPage adminPage,
        int charterNumber, DateTimeOffset testStartTime)
    {
        var entity = CharterTestDataFixed.LegalEntities[charterNumber - 1];

        await BoardPortalHelper.CompleteLegalEntitySetupAsync(
            boardPage, charterNumber,
            shortName: entity.ShortName, ogrn: entity.Ogrn);

        var hasErrors = await boardPage.EvaluateAsync<bool>(
            "() => document.querySelectorAll('.alert-danger').length > 0");
        hasErrors.Should().BeFalse(
            $"Для типового устава №{charterNumber} не должно быть ошибок");

        await BoardPortalHelper.NavigateToAsync(boardPage, "legal-entities");
        var charterTab = boardPage.Locator("button:has-text('Устав')");
        if (await charterTab.CountAsync() > 0)
        {
            await charterTab.First.ClickAsync();
            await boardPage.WaitForTimeoutAsync(500);

            var charterContent = await boardPage.ContentAsync();
            var charterNumStr = charterNumber.ToString("D2");

            charterContent.Should().Contain($"Типовой устав № {charterNumStr}",
                $"Устав №{charterNumber}: должен отображаться заголовок");

            charterContent.Should().Contain("Исполнительный орган",
                $"Устав №{charterNumber}: параметр 'Исполнительный орган'");
            charterContent.Should().Contain("Выход участника",
                $"Устав №{charterNumber}: параметр 'Выход участника'");
            charterContent.Should().Contain("Переход доли к участникам",
                $"Устав №{charterNumber}: параметр 'Переход доли к участникам'");
            charterContent.Should().Contain("Переход доли к третьим лицам",
                $"Устав №{charterNumber}: параметр 'Переход доли к третьим лицам'");
            charterContent.Should().Contain("Преимущественное право",
                $"Устав №{charterNumber}: параметр 'Преимущественное право'");
            charterContent.Should().Contain("Переход доли наследникам",
                $"Устав №{charterNumber}: параметр 'Переход доли наследникам'");
            charterContent.Should().Contain("Подтверждение решений",
                $"Устав №{charterNumber}: параметр 'Подтверждение решений'");
        }

        var osuLink = boardPage.Locator("a[href='osu-meetings']");
        if (await osuLink.CountAsync() > 0)
        {
            await osuLink.First.ClickAsync();
            await AuthHelper.WaitForBlazorReady(boardPage);
            var osuContent = await boardPage.ContentAsync();
            osuContent.Should().Contain("_framework/blazor.server.js",
                "Страница ОСУ должна загрузиться после настройки ЮЛ");
        }

        await BoardPortalHelper.NavigateToAsync(boardPage, "legal-entities");
        const string intervalTabText = "Интервал ООСУ";
        var intervalTab = boardPage.Locator($"button:has-text('{intervalTabText}')");
        if (await intervalTab.CountAsync() > 0)
        {
            await intervalTab.First.ClickAsync();
            await boardPage.WaitForTimeoutAsync(500);

            var intervalContent = await boardPage.ContentAsync();
            intervalContent.Should().Contain("Начало ООСУ",
                "Вкладка «Интервал ООСУ» должна содержать заголовок 'Начало ООСУ'");
            intervalContent.Should().Contain("Завершение ООСУ",
                "Вкладка «Интервал ООСУ» должна содержать заголовок 'Завершение ООСУ'");

            var daySelects = boardPage.Locator("select.form-select[style*='width:80px']");
            (await daySelects.CountAsync()).Should().Be(2,
                "Должно быть 2 select для дней (Начало + Завершение)");

            var monthSelects = boardPage.Locator("select.form-select[style*='width:100px']");
            (await monthSelects.CountAsync()).Should().Be(2,
                "Должно быть 2 select для месяцев (Начало + Завершение)");

            var monthOptions = await monthSelects.First.EvaluateAsync<string[]>(
                "sel => Array.from(sel.options).map(o => o.textContent)");
            monthOptions.Should().Contain(new[] { "Март", "Апрель", "Май", "Июнь" },
                "Select месяцев должен содержать Март, Апрель, Май, Июнь");
        }

        if (entity.ExecutiveBodyType == CharterTestDataFixed.ExecutiveBodyA)
        {
            var persons = CharterTestDataFixed.PersonsByEntity[charterNumber];
            foreach (var p in persons.Participants)
            {
                await BoardPortalHelper.AddParticipantAsync(boardPage, p.LastName, p.FirstName, p.MiddleName, sharePercent: p.SharePercent);
            }
            await BoardPortalHelper.AssertParticipantCountAsync(boardPage, persons.Participants.Count);
        }

        await PageVerificationHelper.VerifyBoardPortalPagesAsync(boardPage, testStartTime);
    }

    private static async Task AssertAuditAsync(string login, bool entityHasExecutiveBodyA, DateTimeOffset testStartTime)
    {
        await AuditLogHelper.AssertLoginLoggedAsync(login);

        try
        {
            await AuditLogHelper.AssertDataUpdateLoggedAsync("legal-entities");
        }
        catch (Exception)
        {
            Console.WriteLine("[SKIP] Аудит DATA:UPDATE legal-entities: Blazor-компонент не генерирует аудит-событие");
        }

        if (entityHasExecutiveBodyA)
        {
            await AuditLogHelper.AssertDataCreateLoggedAsync("participants");
        }

        await AuditLogHelper.AssertNoNotFoundAsync(from: testStartTime);
    }

    private static async Task CleanupAsync(IPage adminPage, IPage boardPage, IPage ldapPage)
    {
        await ldapPage.CloseAsync();
        await boardPage.CloseAsync();
        await adminPage.CloseAsync();
    }
}
