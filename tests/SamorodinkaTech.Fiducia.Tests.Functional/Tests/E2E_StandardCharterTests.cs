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
    /// После установки все последующие тесты прерываются (SkipIfPreviousTestFailed).
    /// volatile — для видимости между потоками xUnit.
    /// </summary>
    private static volatile bool _anyTestFailed;

    // ══════════════════════════════════════════════════════════════════════
    // Тесты: 36 самостоятельных [Fact] (по одному на каждый типовой устав)
    //
    // Уставы 01–06,  19–24: ExecutiveBody A (ГД отдельно + участники)
    // Уставы 07–12,  25–30: ExecutiveBody B (участники = ЕИО)
    // Уставы 13–18,  31–36: ExecutiveBody C (участники = ЕИО совместно)
    // ══════════════════════════════════════════════════════════════════════

    // ── Уставы 01–06 (ExecutiveBody A: ГД + участники) ───────────────
    [Fact] public async Task StandardCharter01_CompleteFlow() => await RunTestAsync(1, "StandardCharter01_CompleteFlow", entityHasExecutiveBodyA: true);
    [Fact] public async Task StandardCharter02_CompleteFlow() => await RunTestAsync(2, "StandardCharter02_CompleteFlow", entityHasExecutiveBodyA: true);
    [Fact] public async Task StandardCharter03_CompleteFlow() => await RunTestAsync(3, "StandardCharter03_CompleteFlow", entityHasExecutiveBodyA: true);
    [Fact] public async Task StandardCharter04_CompleteFlow() => await RunTestAsync(4, "StandardCharter04_CompleteFlow", entityHasExecutiveBodyA: true);
    [Fact] public async Task StandardCharter05_CompleteFlow() => await RunTestAsync(5, "StandardCharter05_CompleteFlow", entityHasExecutiveBodyA: true);
    [Fact] public async Task StandardCharter06_CompleteFlow() => await RunTestAsync(6, "StandardCharter06_CompleteFlow", entityHasExecutiveBodyA: true);

    // ── Уставы 07–12 (ExecutiveBody B: участники = ЕИО) ──────────────
    [Fact] public async Task StandardCharter07_CompleteFlow() => await RunTestAsync(7, "StandardCharter07_CompleteFlow", entityHasExecutiveBodyA: false);
    [Fact] public async Task StandardCharter08_CompleteFlow() => await RunTestAsync(8, "StandardCharter08_CompleteFlow", entityHasExecutiveBodyA: false);
    [Fact] public async Task StandardCharter09_CompleteFlow() => await RunTestAsync(9, "StandardCharter09_CompleteFlow", entityHasExecutiveBodyA: false);
    [Fact] public async Task StandardCharter10_CompleteFlow() => await RunTestAsync(10, "StandardCharter10_CompleteFlow", entityHasExecutiveBodyA: false);
    [Fact] public async Task StandardCharter11_CompleteFlow() => await RunTestAsync(11, "StandardCharter11_CompleteFlow", entityHasExecutiveBodyA: false);
    [Fact] public async Task StandardCharter12_CompleteFlow() => await RunTestAsync(12, "StandardCharter12_CompleteFlow", entityHasExecutiveBodyA: false);

    // ── Уставы 13–18 (ExecutiveBody C: участники = ЕИО совместно) ───
    [Fact] public async Task StandardCharter13_CompleteFlow() => await RunTestAsync(13, "StandardCharter13_CompleteFlow", entityHasExecutiveBodyA: false);
    [Fact] public async Task StandardCharter14_CompleteFlow() => await RunTestAsync(14, "StandardCharter14_CompleteFlow", entityHasExecutiveBodyA: false);
    [Fact] public async Task StandardCharter15_CompleteFlow() => await RunTestAsync(15, "StandardCharter15_CompleteFlow", entityHasExecutiveBodyA: false);
    [Fact] public async Task StandardCharter16_CompleteFlow() => await RunTestAsync(16, "StandardCharter16_CompleteFlow", entityHasExecutiveBodyA: false);
    [Fact] public async Task StandardCharter17_CompleteFlow() => await RunTestAsync(17, "StandardCharter17_CompleteFlow", entityHasExecutiveBodyA: false);
    [Fact] public async Task StandardCharter18_CompleteFlow() => await RunTestAsync(18, "StandardCharter18_CompleteFlow", entityHasExecutiveBodyA: false);

    // ── Уставы 19–24 (ExecutiveBody A: ГД + участники) ──────────────
    [Fact] public async Task StandardCharter19_CompleteFlow() => await RunTestAsync(19, "StandardCharter19_CompleteFlow", entityHasExecutiveBodyA: true);
    [Fact] public async Task StandardCharter20_CompleteFlow() => await RunTestAsync(20, "StandardCharter20_CompleteFlow", entityHasExecutiveBodyA: true);
    [Fact] public async Task StandardCharter21_CompleteFlow() => await RunTestAsync(21, "StandardCharter21_CompleteFlow", entityHasExecutiveBodyA: true);
    [Fact] public async Task StandardCharter22_CompleteFlow() => await RunTestAsync(22, "StandardCharter22_CompleteFlow", entityHasExecutiveBodyA: true);
    [Fact] public async Task StandardCharter23_CompleteFlow() => await RunTestAsync(23, "StandardCharter23_CompleteFlow", entityHasExecutiveBodyA: true);
    [Fact] public async Task StandardCharter24_CompleteFlow() => await RunTestAsync(24, "StandardCharter24_CompleteFlow", entityHasExecutiveBodyA: true);

    // ── Уставы 25–30 (ExecutiveBody B: участники = ЕИО) ─────────────
    [Fact] public async Task StandardCharter25_CompleteFlow() => await RunTestAsync(25, "StandardCharter25_CompleteFlow", entityHasExecutiveBodyA: false);
    [Fact] public async Task StandardCharter26_CompleteFlow() => await RunTestAsync(26, "StandardCharter26_CompleteFlow", entityHasExecutiveBodyA: false);
    [Fact] public async Task StandardCharter27_CompleteFlow() => await RunTestAsync(27, "StandardCharter27_CompleteFlow", entityHasExecutiveBodyA: false);
    [Fact] public async Task StandardCharter28_CompleteFlow() => await RunTestAsync(28, "StandardCharter28_CompleteFlow", entityHasExecutiveBodyA: false);
    [Fact] public async Task StandardCharter29_CompleteFlow() => await RunTestAsync(29, "StandardCharter29_CompleteFlow", entityHasExecutiveBodyA: false);
    [Fact] public async Task StandardCharter30_CompleteFlow() => await RunTestAsync(30, "StandardCharter30_CompleteFlow", entityHasExecutiveBodyA: false);

    // ── Уставы 31–36 (ExecutiveBody C: участники = ЕИО совместно) ──
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
    /// Точка входа для каждого теста. Структура:
    /// 1. Проверить флаг _anyTestFailed — если предыдущий тест упал, пропустить текущий
    /// 2. Выполнить инициализацию (SetupFullCycleAsync)
    /// 3. Выполнить основной flow (ExecuteCharterFlowAsync)
    /// 4. Проверить записи аудита (AssertAuditAsync)
    /// 5. При ошибке: установить флаг _anyTestFailed, залогировать, пробросить исключение
    /// 6. В finally: проверить лог приложения на ошибки + закрыть страницы
    /// </summary>
    private async Task RunTestAsync(int charterNumber, string testName, bool entityHasExecutiveBodyA)
    {
        // Проверяем, не упал ли предыдущий тест — если да, пропускаем текущий
        SkipIfPreviousTestFailed();

        var testStartTime = DateTimeOffset.UtcNow;

        // Инициализация: инфраструктура + сидирование + логин ГД
        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(charterNumber);
        try
        {
            // Основной flow: настройка ЮЛ + устав + участники + проверка страниц
            await ExecuteCharterFlowAsync(boardPage, adminPage, charterNumber, testStartTime);

            // Проверка аудита: вход, изменение данных, создание участников
            await AssertAuditAsync(login, entityHasExecutiveBodyA);
        }
        catch (Exception ex)
        {
            // При ошибке: фиксируем и пробрасываем — xUnit пометит тест как FAILED
            _anyTestFailed = true;
            Console.WriteLine($"[FAIL] {testName}: {ex.Message}");
            throw;
        }
        finally
        {
            // Всегда: проверяем лог приложения на ошибки + закрываем страницы
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    /// <summary>
    /// Канарейка: если предыдущий тест упал (_anyTestFailed = true),
    /// текущий тест прерывается без выполнения.
    /// Исключение ловится xUnit и тест помечается как SKIPPED/FAILED.
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
    /// Первоначальная инициализация теста (выполняется ОДИН раз для всех тестов):
    /// 1. Проверка инфраструктуры (порталы, LDAP, PostgreSQL)
    /// 2. Создание 3 страниц Playwright (Admin Console, Board Portal, LDAP-панель)
    /// 3. Сброс БД + пересоздание LDAP-пользователей (один раз, idempotent)
    /// 4. Сидирование ЮЛ: создание EcosystemParticipant + UserRole + назначение ролей
    /// 5. Логин ГД в Board Portal по login из тестовых данных
    /// </summary>
    private async Task<(IPage adminPage, IPage boardPage, IPage ldapPage, string login)>
        SetupFullCycleAsync(int charterNumber)
    {
        // Убедиться, что Docker-контейнеры (PostgreSQL, LDAP) запущены
        await InfrastructureHelper.EnsureInfrastructureReadyAsync();

        // Создать страницы Playwright для каждого портала
        var adminPage = await CreateAdminConsolePageAsync();
        var boardPage = await CreateBoardPortalPageAsync();
        var ldapPage = await CreatePageAsync();

        // Глобальная инициализация (один раз): сброс БД + пересоздание LDAP-пользователей
        await CharterTestGlobalInit.InitializeAsync(adminPage, ldapPage);

        // Сидирование (один раз для данного ЮЛ): создание EcosystemParticipant + UserRole + ролей
        await CharterTestSeeder.EnsureSeededAsync(adminPage, charterNumber);

        // Получение тестовых данных для ЮЛ (ФИО, логины, доли)
        var persons = CharterTestDataFixed.PersonsByEntity[charterNumber];

        // Логин ГД в Board Portal: если есть ГД — его логин, иначе — первый участник
        var gdLogin = persons.Gd?.Login ?? persons.Participants[0].Login;
        await AuthHelper.LoginAsBoardUserAsync(boardPage, gdLogin);

        // Проверка: после логина должны оказаться на главной странице /main
        boardPage.Url.Should().Contain("/main");

        return (adminPage, boardPage, ldapPage, gdLogin);
    }

    /// <summary>
    /// Основной flow теста (после инициализации):
    /// 1. Заполнение полей ЮЛ + выбор ОКОПФ + выбор типового устава + сохранение
    /// 2. Проверка: на странице нет ошибок (.alert-danger)
    /// 3. Переход на страницу ОСУ — проверка доступности после настройки ЮЛ
    /// 4. Добавление участников (только для ExecutiveBody A — ГД отдельно)
    /// 5. Проверка страниц Board Portal (US-002..US-024) + Admin Console
    /// </summary>
    private static async Task ExecuteCharterFlowAsync(
        IPage boardPage, IPage adminPage,
        int charterNumber, DateTimeOffset testStartTime)
    {
        // Получение данных ЮЛ (наименование, ИНН, тип исполнительного органа)
        var entity = CharterTestDataFixed.LegalEntities[charterNumber - 1];

        // Шаг 1: Настройка ЮЛ — заполнение карточки + ОКОПФ + выбор устава + сохранение
        await BoardPortalHelper.CompleteLegalEntitySetupAsync(
            boardPage, charterNumber,
            shortName: entity.ShortName, ogrn: entity.Ogrn);

        // Шаг 2: Проверка — на странице не должно быть блоков ошибок (.alert-danger)
        var hasErrors = await boardPage.EvaluateAsync<bool>(
            "() => document.querySelectorAll('.alert-danger').length > 0");
        hasErrors.Should().BeFalse(
            $"Для типового устава №{charterNumber} не должно быть ошибок");

        // Шаг 3: Переход на ОСУ — страница должна загрузиться после настройки ЮЛ
        await BoardPortalHelper.NavigateToAsync(boardPage, "osu-meetings");
        var osuContent = await boardPage.ContentAsync();
        osuContent.Should().Contain("_framework/blazor.server.js",
            "Страница ОСУ должна загрузиться после настройки ЮЛ");

        // Шаг 4: Добавление участников (только для ExecutiveBody A — ГД отдельно от участников)
        if (entity.ExecutiveBodyType == CharterTestDataFixed.ExecutiveBodyA)
        {
            var persons = CharterTestDataFixed.PersonsByEntity[charterNumber];
            foreach (var p in persons.Participants)
            {
                // Добавление каждого участника через API (POST /api/participants)
                await BoardPortalHelper.AddParticipantAsync(boardPage, p.FullName, sharePercent: p.SharePercent);
            }
            // Проверка: количество участников в БД должно совпадать с ожидаемым
            await BoardPortalHelper.AssertParticipantCountAsync(boardPage, persons.Participants.Count);
        }

        // Шаг 5: Проверка основных страниц Board Portal и Admin Console
        // (навигация через UI-элементы, проверка контента и записей аудита)
        await PageVerificationHelper.VerifyBoardPortalPagesAsync(boardPage, testStartTime);
        await PageVerificationHelper.VerifyAdminConsolePagesAsync(adminPage, testStartTime);
    }

    /// <summary>
    /// Проверка записей в логе аудита после выполнения flow:
    /// 1. Вход в систему (LOGIN_SUCCESS) — должен быть залогирован для данного логина
    /// 2. Изменение данных ЮЛ (DATA:UPDATE legal-entities) — сохранение настроек
    /// 3. Создание участников (DATA:CREATE participants) — только для ExecutiveBody A
    /// 4. Отсутствие ошибок доступа (ACCESS:PAGE_DENIED) — все переходы разрешены
    /// </summary>
    private static async Task AssertAuditAsync(string login, bool entityHasExecutiveBodyA)
    {
        // Вход в систему должен быть залогирован в аудите
        await AuditLogHelper.AssertLoginLoggedAsync(login);

        // Сохранение настроек ЮЛ (ОКОПФ, устав) должно быть залогировано
        await AuditLogHelper.AssertDataUpdateLoggedAsync("legal-entities");

        // Добавление участников должно быть залогировано (только для типа A — ГД отдельно)
        if (entityHasExecutiveBodyA)
        {
            await AuditLogHelper.AssertDataCreateLoggedAsync("participants");
        }

        // Не должно быть записей об отказе в доступе — все переходы были разрешены
        await AuditLogHelper.AssertNoAccessDeniedAsync();
    }

    /// <summary>
    /// Очистка: закрытие всех 3 страниц Playwright.
    /// Выполняется в finally — гарантированно, даже при ошибке.
    /// </summary>
    private static async Task CleanupAsync(IPage adminPage, IPage boardPage, IPage ldapPage)
    {
        await ldapPage.CloseAsync();
        await boardPage.CloseAsync();
        await adminPage.CloseAsync();
    }
}
