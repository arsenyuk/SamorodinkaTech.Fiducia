using FluentAssertions;
using Microsoft.Playwright;
using SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

namespace SamorodinkaTech.Fiducia.Tests.Functional;

/// <summary>
/// Сквозные E2E-тесты для 36 типовых уставов ООО.
/// БД сбрасывается ОДИН раз перед прогоном всех тестов.
/// Каждый тест работает со своим фиксированным ЮЛ и набором лиц.
/// Запрещено параллельное исполнение (Collection "CharterTests").
/// При ошибке в одном тесте все последующие тесты прерываются.
/// </summary>
[Collection("CharterTests")]
public class E2E_StandardCharterTests : BrowserFixture
{
    /// <summary>
    /// Флаг: хотя бы один тест завершился ошибкой.
    /// После установки все последующие тесты прерываются.
    /// </summary>
    private static volatile bool _anyTestFailed;

    // ══════════════════════════════════════════════════════════════════════
    // Тесты: 36 самостоятельных [Fact] (по одному на каждый типовой устав)
    // ══════════════════════════════════════════════════════════════════════

    [Fact] public async Task StandardCharter01_CompleteFlow() => await RunTestAsync(1, "StandardCharter01_CompleteFlow", entityHasExecutiveBodyA: true);
    [Fact] public async Task StandardCharter02_CompleteFlow() => await RunTestAsync(2, "StandardCharter02_CompleteFlow", entityHasExecutiveBodyA: true);
    [Fact] public async Task StandardCharter03_CompleteFlow() => await RunTestAsync(3, "StandardCharter03_CompleteFlow", entityHasExecutiveBodyA: true);
    [Fact] public async Task StandardCharter04_CompleteFlow() => await RunTestAsync(4, "StandardCharter04_CompleteFlow", entityHasExecutiveBodyA: true);
    [Fact] public async Task StandardCharter05_CompleteFlow() => await RunTestAsync(5, "StandardCharter05_CompleteFlow", entityHasExecutiveBodyA: true);
    [Fact] public async Task StandardCharter06_CompleteFlow() => await RunTestAsync(6, "StandardCharter06_CompleteFlow", entityHasExecutiveBodyA: true);
    [Fact] public async Task StandardCharter07_CompleteFlow() => await RunTestAsync(7, "StandardCharter07_CompleteFlow", entityHasExecutiveBodyA: false);
    [Fact] public async Task StandardCharter08_CompleteFlow() => await RunTestAsync(8, "StandardCharter08_CompleteFlow", entityHasExecutiveBodyA: false);
    [Fact] public async Task StandardCharter09_CompleteFlow() => await RunTestAsync(9, "StandardCharter09_CompleteFlow", entityHasExecutiveBodyA: false);
    [Fact] public async Task StandardCharter10_CompleteFlow() => await RunTestAsync(10, "StandardCharter10_CompleteFlow", entityHasExecutiveBodyA: false);
    [Fact] public async Task StandardCharter11_CompleteFlow() => await RunTestAsync(11, "StandardCharter11_CompleteFlow", entityHasExecutiveBodyA: false);
    [Fact] public async Task StandardCharter12_CompleteFlow() => await RunTestAsync(12, "StandardCharter12_CompleteFlow", entityHasExecutiveBodyA: false);
    [Fact] public async Task StandardCharter13_CompleteFlow() => await RunTestAsync(13, "StandardCharter13_CompleteFlow", entityHasExecutiveBodyA: false);
    [Fact] public async Task StandardCharter14_CompleteFlow() => await RunTestAsync(14, "StandardCharter14_CompleteFlow", entityHasExecutiveBodyA: false);
    [Fact] public async Task StandardCharter15_CompleteFlow() => await RunTestAsync(15, "StandardCharter15_CompleteFlow", entityHasExecutiveBodyA: false);
    [Fact] public async Task StandardCharter16_CompleteFlow() => await RunTestAsync(16, "StandardCharter16_CompleteFlow", entityHasExecutiveBodyA: false);
    [Fact] public async Task StandardCharter17_CompleteFlow() => await RunTestAsync(17, "StandardCharter17_CompleteFlow", entityHasExecutiveBodyA: false);
    [Fact] public async Task StandardCharter18_CompleteFlow() => await RunTestAsync(18, "StandardCharter18_CompleteFlow", entityHasExecutiveBodyA: false);
    [Fact] public async Task StandardCharter19_CompleteFlow() => await RunTestAsync(19, "StandardCharter19_CompleteFlow", entityHasExecutiveBodyA: true);
    [Fact] public async Task StandardCharter20_CompleteFlow() => await RunTestAsync(20, "StandardCharter20_CompleteFlow", entityHasExecutiveBodyA: true);
    [Fact] public async Task StandardCharter21_CompleteFlow() => await RunTestAsync(21, "StandardCharter21_CompleteFlow", entityHasExecutiveBodyA: true);
    [Fact] public async Task StandardCharter22_CompleteFlow() => await RunTestAsync(22, "StandardCharter22_CompleteFlow", entityHasExecutiveBodyA: true);
    [Fact] public async Task StandardCharter23_CompleteFlow() => await RunTestAsync(23, "StandardCharter23_CompleteFlow", entityHasExecutiveBodyA: true);
    [Fact] public async Task StandardCharter24_CompleteFlow() => await RunTestAsync(24, "StandardCharter24_CompleteFlow", entityHasExecutiveBodyA: true);
    [Fact] public async Task StandardCharter25_CompleteFlow() => await RunTestAsync(25, "StandardCharter25_CompleteFlow", entityHasExecutiveBodyA: false);
    [Fact] public async Task StandardCharter26_CompleteFlow() => await RunTestAsync(26, "StandardCharter26_CompleteFlow", entityHasExecutiveBodyA: false);
    [Fact] public async Task StandardCharter27_CompleteFlow() => await RunTestAsync(27, "StandardCharter27_CompleteFlow", entityHasExecutiveBodyA: false);
    [Fact] public async Task StandardCharter28_CompleteFlow() => await RunTestAsync(28, "StandardCharter28_CompleteFlow", entityHasExecutiveBodyA: false);
    [Fact] public async Task StandardCharter29_CompleteFlow() => await RunTestAsync(29, "StandardCharter29_CompleteFlow", entityHasExecutiveBodyA: false);
    [Fact] public async Task StandardCharter30_CompleteFlow() => await RunTestAsync(30, "StandardCharter30_CompleteFlow", entityHasExecutiveBodyA: false);
    [Fact] public async Task StandardCharter31_CompleteFlow() => await RunTestAsync(31, "StandardCharter31_CompleteFlow", entityHasExecutiveBodyA: false);
    [Fact] public async Task StandardCharter32_CompleteFlow() => await RunTestAsync(32, "StandardCharter32_CompleteFlow", entityHasExecutiveBodyA: false);
    [Fact] public async Task StandardCharter33_CompleteFlow() => await RunTestAsync(33, "StandardCharter33_CompleteFlow", entityHasExecutiveBodyA: false);
    [Fact] public async Task StandardCharter34_CompleteFlow() => await RunTestAsync(34, "StandardCharter34_CompleteFlow", entityHasExecutiveBodyA: false);
    [Fact] public async Task StandardCharter35_CompleteFlow() => await RunTestAsync(35, "StandardCharter35_CompleteFlow", entityHasExecutiveBodyA: false);
    [Fact] public async Task StandardCharter36_CompleteFlow() => await RunTestAsync(36, "StandardCharter36_CompleteFlow", entityHasExecutiveBodyA: false);

    // ══════════════════════════════════════════════════════════════════════
    // Вспомогательные методы
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Единый запуск теста: проверка флага → инициализация → flow → аудит → cleanup.
    /// При ошибке: флаг _anyTestFailed = true, тест падает, все последующие пропускаются.
    /// </summary>
    private async Task RunTestAsync(int charterNumber, string testName, bool entityHasExecutiveBodyA)
    {
        SkipIfPreviousTestFailed();

        var testStartTime = DateTimeOffset.UtcNow;

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(charterNumber);
        try
        {
            await ExecuteCharterFlowAsync(boardPage, adminPage, charterNumber, testStartTime);
            await AssertAuditAsync(login, entityHasExecutiveBodyA);
        }
        catch (Exception ex)
        {
            _anyTestFailed = true;
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

    /// <summary>
    /// Если предыдущий тест упал — прервать текущий тест без выполнения.
    /// </summary>
    private static void SkipIfPreviousTestFailed()
    {
        if (_anyTestFailed)
        {
            throw new InvalidOperationException(
                "Предыдущий тест завершился ошибкой — прогон прерван.");
        }
    }

    /// <summary>
    /// Первоначальная инициализация: инфраструктура + БД + LDAP + сидирование.
    /// Выполняется ОДИН раз (idempotent через флаги в CharterTestGlobalInit/CharterTestSeeder).
    /// Создаёт 3 страницы и логинит ГД для указанного ЮЛ.
    /// </summary>
    private async Task<(IPage adminPage, IPage boardPage, IPage ldapPage, string login)>
        SetupFullCycleAsync(int charterNumber)
    {
        await InfrastructureHelper.EnsureInfrastructureReadyAsync();

        var adminPage = await CreateAdminConsolePageAsync();
        var boardPage = await CreateBoardPortalPageAsync();
        var ldapPage = await CreatePageAsync();

        // Глобальная инициализация (один раз): сброс БД + LDAP
        await CharterTestGlobalInit.InitializeAsync(adminPage, ldapPage);

        // Сидирование (один раз): создание ЮЛ + пользователей + ролей
        await CharterTestSeeder.EnsureSeededAsync(adminPage, charterNumber);

        // Получение данных для данного ЮЛ
        var persons = CharterTestDataFixed.PersonsByEntity[charterNumber];

        // Логин ГД в Board Portal
        var gdLogin = persons.Gd?.Login ?? persons.Participants[0].Login;
        await AuthHelper.LoginAsBoardUserAsync(boardPage, gdLogin);
        boardPage.Url.Should().Contain("/main");

        return (adminPage, boardPage, ldapPage, gdLogin);
    }

    /// <summary>
    /// Основной flow: заполнение ЮЛ + выбор устава + сохранение + участники + проверка страниц.
    /// </summary>
    private static async Task ExecuteCharterFlowAsync(
        IPage boardPage, IPage adminPage,
        int charterNumber, DateTimeOffset testStartTime)
    {
        var entity = CharterTestDataFixed.LegalEntities[charterNumber - 1];

        // Заполнение полей ЮЛ + выбор типового устава + сохранение
        await BoardPortalHelper.CompleteLegalEntitySetupAsync(
            boardPage, charterNumber,
            shortName: entity.ShortName, ogrn: entity.Ogrn);

        // Проверка отсутствия ошибок
        var hasErrors = await boardPage.EvaluateAsync<bool>(
            "() => document.querySelectorAll('.alert-danger').length > 0");
        hasErrors.Should().BeFalse(
            $"Для типового устава №{charterNumber} не должно быть ошибок");

        // Добавление участников (для ExecutiveBody A)
        if (entity.ExecutiveBodyType == CharterTestDataFixed.ExecutiveBodyA)
        {
            var persons = CharterTestDataFixed.PersonsByEntity[charterNumber];
            foreach (var p in persons.Participants)
            {
                await BoardPortalHelper.AddParticipantAsync(boardPage, p.FullName, sharePercent: p.SharePercent);
            }
            await BoardPortalHelper.AssertParticipantCountAsync(boardPage, persons.Participants.Count);
        }

        // Проверка страниц Board Portal + Admin Console
        await PageVerificationHelper.VerifyBoardPortalPagesAsync(boardPage, testStartTime);
        await PageVerificationHelper.VerifyAdminConsolePagesAsync(adminPage, testStartTime);
    }

    /// <summary>
    /// Проверка записей аудита: вход, изменение данных, участники, отсутствие ошибок доступа.
    /// </summary>
    private static async Task AssertAuditAsync(string login, bool entityHasExecutiveBodyA)
    {
        await AuditLogHelper.AssertLoginLoggedAsync(login);
        await AuditLogHelper.AssertDataUpdateLoggedAsync("legal-entities");

        if (entityHasExecutiveBodyA)
        {
            await AuditLogHelper.AssertDataCreateLoggedAsync("participants");
        }

        await AuditLogHelper.AssertNoAccessDeniedAsync();
    }

    /// <summary>
    private static async Task CleanupAsync(IPage adminPage, IPage boardPage, IPage ldapPage)
    {
        await ldapPage.CloseAsync();
        await boardPage.CloseAsync();
        await adminPage.CloseAsync();
    }
}
