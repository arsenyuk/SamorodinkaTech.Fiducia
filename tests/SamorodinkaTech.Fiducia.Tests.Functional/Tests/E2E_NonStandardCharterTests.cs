using FluentAssertions;
using Microsoft.Playwright;
using SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

namespace SamorodinkaTech.Fiducia.Tests.Functional;

/// <summary>
/// Сквозные E2E-тесты для нетипового (индивидуального) устава ООО.
/// БД сбрасывается ОДИН раз перед прогоном всех тестов.
/// Каждый тест работает со своим фиксированным ЮЛ и набором лиц.
/// Запрещено параллельное исполнение (Collection "CharterTests").
/// Тестовый сценарий:
/// 1. Логин ГД в Board Portal (пользователь уже создан при сидировании)
/// 2. Заполнение полей ЮЛ + выбор нетипового устава
/// 3. Настройка конкретного параметра устава
/// 4. Добавление участников
/// 5. Сохранение и проверка отсутствия ошибок
/// 6. Проверка записей аудита (вход, чтение/запись, участники)
/// 7. Проверка отсутствия ошибок в логе приложения за период работы теста
/// </summary>
[Collection("CharterTests")]
public class E2E_NonStandardCharterTests : BrowserFixture
{
    // ══════════════════════════════════════════════════════════════════════
    // Параметры нетипового устава ( фиксированные значения )
    // ══════════════════════════════════════════════════════════════════════

    private const string ExitAllowed = "true";
    private const string ExitMinSharePercent = "5";
    private const string ExitMaxSharePercent = "40";
    private const string ExitConditionDescription = "по истечении 2 лет с момента вступления";
    private const string ExitRequiresUnanimousOsu = "true";
    private const string TransferToParticipants = "true";
    private const string TransferToThirdParties = "CONSENT";
    private const string PreemptiveRight = "true";
    private const string InheritanceWithoutConsent = "true";
    private const string ExecutiveBody = "A";
    private const string HasBoardOfDirectors = "true";
    private const string BoardDecidesConveningOsu = "true";
    private const string VosuThresholdPercent = "15";

    // ══════════════════════════════════════════════════════════════════════
    // Тесты
    // ══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task NonStandardCharter_ExitAllowed_ShouldSaveWithoutErrors()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "NonStandardCharter_ExitAllowed";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(37);
        try
        {
            await BoardPortalHelper.SelectNonStandardCharterAsync(boardPage);
            await BoardPortalHelper.AssertNonStandardCharterFieldsVisibleAsync(boardPage);
            await BoardPortalHelper.ConfigureCharterParameterAsync(
                boardPage, "exit-allowed", ExitAllowed);
            await AddParticipantsAsync(boardPage, 37);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);

            await AssertAuditForNonStandardCharterAsync(login, participantsAdded: true);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task NonStandardCharter_ExitMinSharePercent_ShouldSaveWithoutErrors()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "NonStandardCharter_ExitMinSharePercent";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(38);
        try
        {
            await BoardPortalHelper.SelectNonStandardCharterAsync(boardPage);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "exit-allowed", ExitAllowed);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "exit-min-share", ExitMinSharePercent);
            await AddParticipantsAsync(boardPage, 38);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);

            await AssertAuditForNonStandardCharterAsync(login, participantsAdded: true);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task NonStandardCharter_ExitMaxSharePercent_ShouldSaveWithoutErrors()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "NonStandardCharter_ExitMaxSharePercent";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(39);
        try
        {
            await BoardPortalHelper.SelectNonStandardCharterAsync(boardPage);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "exit-allowed", ExitAllowed);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "exit-max-share", ExitMaxSharePercent);
            await AddParticipantsAsync(boardPage, 39);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);

            await AssertAuditForNonStandardCharterAsync(login, participantsAdded: true);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task NonStandardCharter_ExitConditionDescription_ShouldSaveWithoutErrors()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "NonStandardCharter_ExitConditionDescription";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(40);
        try
        {
            await BoardPortalHelper.SelectNonStandardCharterAsync(boardPage);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "exit-allowed", ExitAllowed);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "exit-condition", ExitConditionDescription);
            await AddParticipantsAsync(boardPage, 40);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);

            await AssertAuditForNonStandardCharterAsync(login, participantsAdded: true);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task NonStandardCharter_ExitRequiresUnanimousOsu_ShouldSaveWithoutErrors()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "NonStandardCharter_ExitRequiresUnanimousOsu";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(41);
        try
        {
            await BoardPortalHelper.SelectNonStandardCharterAsync(boardPage);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "exit-allowed", ExitAllowed);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "exit-unanimous", ExitRequiresUnanimousOsu);
            await AddParticipantsAsync(boardPage, 41);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);

            await AssertAuditForNonStandardCharterAsync(login, participantsAdded: true);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task NonStandardCharter_TransferToParticipants_ShouldSaveWithoutErrors()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "NonStandardCharter_TransferToParticipants";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(42);
        try
        {
            await BoardPortalHelper.SelectNonStandardCharterAsync(boardPage);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "transfer-participants", TransferToParticipants);
            await AddParticipantsAsync(boardPage, 42);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);

            await AssertAuditForNonStandardCharterAsync(login, participantsAdded: true);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task NonStandardCharter_TransferToThirdParties_ShouldSaveWithoutErrors()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "NonStandardCharter_TransferToThirdParties";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(43);
        try
        {
            await BoardPortalHelper.SelectNonStandardCharterAsync(boardPage);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "transfer-third-parties", TransferToThirdParties);
            await AddParticipantsAsync(boardPage, 43);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);

            await AssertAuditForNonStandardCharterAsync(login, participantsAdded: true);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task NonStandardCharter_PreemptiveRight_ShouldSaveWithoutErrors()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "NonStandardCharter_PreemptiveRight";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(44);
        try
        {
            await BoardPortalHelper.SelectNonStandardCharterAsync(boardPage);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "preemptive-right", PreemptiveRight);
            await AddParticipantsAsync(boardPage, 44);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);

            await AssertAuditForNonStandardCharterAsync(login, participantsAdded: true);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task NonStandardCharter_InheritanceWithoutConsent_ShouldSaveWithoutErrors()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "NonStandardCharter_InheritanceWithoutConsent";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(45);
        try
        {
            await BoardPortalHelper.SelectNonStandardCharterAsync(boardPage);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "inheritance", InheritanceWithoutConsent);
            await AddParticipantsAsync(boardPage, 45);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);

            await AssertAuditForNonStandardCharterAsync(login, participantsAdded: true);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task NonStandardCharter_ExecutiveBody_ShouldSaveWithoutErrors()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "NonStandardCharter_ExecutiveBody";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(46);
        try
        {
            await BoardPortalHelper.SelectNonStandardCharterAsync(boardPage);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "executive-body", ExecutiveBody);
            await AddParticipantsAsync(boardPage, 46);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);

            await AssertAuditForNonStandardCharterAsync(login, participantsAdded: true);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task NonStandardCharter_HasBoardOfDirectors_ShouldSaveAndShowBoardTab()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "NonStandardCharter_HasBoardOfDirectors";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(47);
        try
        {
            await BoardPortalHelper.SelectNonStandardCharterAsync(boardPage);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "has-board", HasBoardOfDirectors);
            await BoardPortalHelper.AssertBoardOfDirectorsAvailableAsync(boardPage);
            await AddParticipantsAsync(boardPage, 47);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);

            await AssertAuditForNonStandardCharterAsync(login, participantsAdded: true);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task NonStandardCharter_BoardDecidesConveningOsu_ShouldSaveWithoutErrors()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "NonStandardCharter_BoardDecidesConveningOsu";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(48);
        try
        {
            await BoardPortalHelper.SelectNonStandardCharterAsync(boardPage);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "has-board", HasBoardOfDirectors);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "board-convenes-osu", BoardDecidesConveningOsu);
            await AddParticipantsAsync(boardPage, 48);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);

            await AssertAuditForNonStandardCharterAsync(login, participantsAdded: true);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task NonStandardCharter_VosuThresholdPercent_ShouldSaveWithoutErrors()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "NonStandardCharter_VosuThresholdPercent";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(49);
        try
        {
            await BoardPortalHelper.SelectNonStandardCharterAsync(boardPage);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "has-board", HasBoardOfDirectors);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "vosu-threshold", VosuThresholdPercent);
            await AddParticipantsAsync(boardPage, 49);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);

            await AssertAuditForNonStandardCharterAsync(login, participantsAdded: true);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task NonStandardCharter_AllParameters_ShouldSaveWithoutErrors()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "NonStandardCharter_AllParameters";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(50);
        try
        {
            await BoardPortalHelper.SelectNonStandardCharterAsync(boardPage);
            await BoardPortalHelper.AssertNonStandardCharterFieldsVisibleAsync(boardPage);

            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "exit-allowed", ExitAllowed);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "exit-min-share", ExitMinSharePercent);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "exit-max-share", ExitMaxSharePercent);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "exit-condition", ExitConditionDescription);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "exit-unanimous", ExitRequiresUnanimousOsu);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "transfer-participants", TransferToParticipants);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "transfer-third-parties", TransferToThirdParties);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "preemptive-right", PreemptiveRight);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "inheritance", InheritanceWithoutConsent);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "executive-body", ExecutiveBody);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "has-board", HasBoardOfDirectors);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "board-convenes-osu", BoardDecidesConveningOsu);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "vosu-threshold", VosuThresholdPercent);

            await BoardPortalHelper.AssertBoardOfDirectorsAvailableAsync(boardPage);
            await AddParticipantsAsync(boardPage, 50);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);

            await AssertAuditForNonStandardCharterAsync(login, participantsAdded: true);
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

    private async Task<(IPage adminPage, IPage boardPage, IPage ldapPage, string login)> SetupFullCycleAsync(int entityIndex)
    {
        // Инфраструктура (порталы, LDAP) должна быть запущена ДО создания страниц
        await InfrastructureHelper.EnsureInfrastructureReadyAsync();

        var adminPage = await CreateAdminConsolePageAsync();
        var boardPage = await CreateBoardPortalPageAsync();
        var ldapPage = await CreatePageAsync();

        // Глобальная инициализация: инфраструктура + БД + LDAP (один раз)
        await CharterTestGlobalInit.InitializeAsync(adminPage, ldapPage);

        // Сидирование: логин + создание ЮЛ + роли (один раз)
        await CharterTestSeeder.EnsureSeededAsync(adminPage, ldapPage);

        // Получение фиксированных данных
        var entity = CharterTestDataFixed.LegalEntities[entityIndex - 1];
        var persons = CharterTestDataFixed.PersonsByEntity[entityIndex];

        // Логин ГД в Board Portal
        var gdLogin = persons.Gd?.Uid ?? persons.Participants[0].Uid;
        await AuthHelper.LoginAsBoardUserAsync(boardPage, gdLogin);
        boardPage.Url.Should().Contain("/main");

        // Заполнение полей ЮЛ
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

    /// <summary>
    /// Проверить записи аудита для нетипового устава: вход, сохранение, участники.
    /// </summary>
    private static async Task AssertAuditForNonStandardCharterAsync(string login, bool participantsAdded)
    {
        // Вход в систему должен быть залогирован
        await AuditLogHelper.AssertLoginLoggedAsync(login);

        // Сохранение данных ЮЛ должно быть залогировано
        await AuditLogHelper.AssertDataUpdateLoggedAsync("legal-entities");

        // Добавление участников должно быть залогировано
        if (participantsAdded)
        {
            await AuditLogHelper.AssertDataCreateLoggedAsync("participants");
        }

        // Не должно быть ошибок доступа
        await AuditLogHelper.AssertNoAccessDeniedAsync();
    }
}
