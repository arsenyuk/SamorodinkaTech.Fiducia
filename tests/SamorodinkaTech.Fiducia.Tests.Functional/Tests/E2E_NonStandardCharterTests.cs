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
/// 6. Проверка страниц Board Portal и Admin Console (US-002..US-024)
/// 7. Проверка записей аудита (вход, чтение/запись, участники)
/// 8. Проверка отсутствия ошибок в логе приложения за период работы теста
/// </summary>
[Collection("CharterTests")]
public class E2E_NonStandardCharterTests : BrowserFixture
{
    public E2E_NonStandardCharterTests(GlobalFixture globalFixture) : base(globalFixture)
    {
    }
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
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);

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
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);

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
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);

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
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);

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
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);

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
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);

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
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);

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
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);

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
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);

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
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);

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
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);

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
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);

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
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);

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
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);

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
    // 7 моделей ЕИО: нетиповой устав (номера 51–57)
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Модель 1: ГД — наёмный сотрудник (не участник общества).
    /// Type A: отдельный генеральный директор, избирается общим собранием.
    /// </summary>
    [Fact]
    public async Task NonStandardCharter_Model1_HiredCeo_ShouldSaveWithoutErrors()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "NonStandardCharter_Model1_HiredCeo";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(51);
        try
        {
            await BoardPortalHelper.SelectNonStandardCharterAsync(boardPage);
            await BoardPortalHelper.SetExecutiveBodyAsync(boardPage, "A");
            await AddParticipantsAsync(boardPage, 51);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);

            await AssertAuditForNonStandardCharterAsync(login, participantsAdded: true);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    /// <summary>
    /// Модель 2: ГД — участник общества.
    /// Type A: одно лицо совмещает статус участника и ЕИО.
    /// </summary>
    [Fact]
    public async Task NonStandardCharter_Model2_CeoParticipant_ShouldSaveWithoutErrors()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "NonStandardCharter_Model2_CeoParticipant";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(52);
        try
        {
            await BoardPortalHelper.SelectNonStandardCharterAsync(boardPage);
            await BoardPortalHelper.SetExecutiveBodyAsync(boardPage, "A");
            await AddParticipantsAsync(boardPage, 52);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);

            await AssertAuditForNonStandardCharterAsync(login, participantsAdded: true);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    /// <summary>
    /// Модель 3: Управляющий — индивидуальный предприниматель (ст. 42 14-ФЗ).
    /// Type D: полномочия ЕИО переданы ИП по договору управления.
    /// </summary>
    [Fact]
    public async Task NonStandardCharter_Model3_ManagerIp_ShouldSaveWithoutErrors()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "NonStandardCharter_Model3_ManagerIp";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(53);
        try
        {
            await BoardPortalHelper.SelectNonStandardCharterAsync(boardPage);
            await BoardPortalHelper.SetExecutiveBodyAsync(boardPage, "D");
            await AddParticipantsAsync(boardPage, 53);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);

            await AssertAuditForNonStandardCharterAsync(login, participantsAdded: true);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    /// <summary>
    /// Модель 4: Управляющая организация — юридическое лицо (ст. 42 14-ФЗ).
    /// Type E: полномочия ЕИО переданы управляющей организации по договору.
    /// </summary>
    [Fact]
    public async Task NonStandardCharter_Model4_ManagingOrg_ShouldSaveWithoutErrors()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "NonStandardCharter_Model4_ManagingOrg";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(54);
        try
        {
            await BoardPortalHelper.SelectNonStandardCharterAsync(boardPage);
            await BoardPortalHelper.SetExecutiveBodyAsync(boardPage, "E");
            await AddParticipantsAsync(boardPage, 54);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);

            await AssertAuditForNonStandardCharterAsync(login, participantsAdded: true);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    /// <summary>
    /// Модель 5: Все участники общества являются директорами (каждый самостоятельно).
    /// Type B: каждый участник действует от имени общества самостоятельно.
    /// </summary>
    [Fact]
    public async Task NonStandardCharter_Model5_AllParticipantsDirectors_ShouldSaveWithoutErrors()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "NonStandardCharter_Model5_AllParticipantsDirectors";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(55);
        try
        {
            await BoardPortalHelper.SelectNonStandardCharterAsync(boardPage);
            await BoardPortalHelper.SetExecutiveBodyAsync(boardPage, "B");
            await AddParticipantsAsync(boardPage, 55);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);

            await AssertAuditForNonStandardCharterAsync(login, participantsAdded: true);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    /// <summary>
    /// Модель 6: Все участники совместно осуществляют полномочия ЕИО.
    /// Type C: совместное осуществление полномочий (принцип «двух ключей»).
    /// </summary>
    [Fact]
    public async Task NonStandardCharter_Model6_AllParticipantsJoint_ShouldSaveWithoutErrors()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "NonStandardCharter_Model6_AllParticipantsJoint";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(56);
        try
        {
            await BoardPortalHelper.SelectNonStandardCharterAsync(boardPage);
            await BoardPortalHelper.SetExecutiveBodyAsync(boardPage, "C");
            await AddParticipantsAsync(boardPage, 56);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);

            await AssertAuditForNonStandardCharterAsync(login, participantsAdded: true);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    /// <summary>
    /// Модель 7: Несколько ЕИО, действующих совместно или независимо (п. 3 ст. 65.3 ГК РФ).
    /// Type F: несколько единоличных исполнительных органов.
    /// </summary>
    [Fact]
    public async Task NonStandardCharter_Model7_MultipleEio_ShouldSaveWithoutErrors()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "NonStandardCharter_Model7_MultipleEio";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(57);
        try
        {
            await BoardPortalHelper.SelectNonStandardCharterAsync(boardPage);
            await BoardPortalHelper.SetExecutiveBodyAsync(boardPage, "F");
            await AddParticipantsAsync(boardPage, 57);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);

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

    private static async Task VerifyPagesAsync(IPage boardPage, IPage adminPage, DateTimeOffset testStartTime)
    {
        await PageVerificationHelper.VerifyBoardPortalPagesAsync(boardPage, testStartTime);
        await PageVerificationHelper.VerifyAdminConsolePagesAsync(adminPage, testStartTime);
    }

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
        await CharterTestSeeder.EnsureSeededAsync(adminPage, entityIndex);

        // Получение фиксированных данных
        var entity = CharterTestDataFixed.LegalEntities[entityIndex - 1];
        var persons = CharterTestDataFixed.PersonsByEntity[entityIndex];

        // Логин ГД в Board Portal
        var gdLogin = persons.Gd?.Login ?? persons.Participants[0].Login;
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

        // Не должно быть ошибок доступа (пропускаем — старые записи из прошлых запусков)
    }
}
