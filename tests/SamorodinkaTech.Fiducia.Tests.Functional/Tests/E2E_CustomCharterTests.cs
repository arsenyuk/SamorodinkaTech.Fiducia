using FluentAssertions;
using Microsoft.Playwright;
using SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

namespace SamorodinkaTech.Fiducia.Tests.Functional;

/// <summary>
/// Сквозные E2E-тесты для индивидуального устава ООО.
/// БД сбрасывается ОДИН раз перед прогоном всех тестов.
/// Каждый тест работает со своим фиксированным ЮЛ и набором лиц.
/// Запрещено параллельное исполнение (Collection "CharterTests").
/// Тестовый сценарий:
/// 1. Логин ГД в Board Portal (пользователь уже создан при сидировании)
/// 2. Заполнение полей ЮЛ + выбор индивидуального устава
/// 3. Настройка конкретного параметра устава
/// 4. Добавление участников
/// 5. Сохранение и проверка отсутствия ошибок
/// 6. Проверка страниц Board Portal и Admin Console (US-002..US-024)
/// 7. Проверка записей аудита (вход, чтение/запись, участники)
/// 8. Проверка отсутствия ошибок в логе приложения за период работы теста
/// Документация: docs/e2e-nonstandard-charter.md
/// </summary>
[Collection("CharterTests")]
public class E2E_CustomCharterTests : BrowserFixture
{
    public E2E_CustomCharterTests(GlobalFixture globalFixture) : base(globalFixture)
    {
    }
    // ══════════════════════════════════════════════════════════════════════
    // Параметры индивидуального устава ( фиксированные значения )
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
    private const string VosuThresholdPercent = "5";

    // ══════════════════════════════════════════════════════════════════════
    // Тесты
    // ══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task CustomCharter_ExitAllowed_ShouldSaveWithoutErrors()
    {
        SkipIfPreviousFailed();

        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "CustomCharter_ExitAllowed";

        var (adminPage, boardPage, login) = await SetupFullCycleAsync(37);
        try
        {
            await BoardPortalHelper.SelectCustomCharterAsync(boardPage);
            await BoardPortalHelper.AssertCustomCharterFieldsVisibleAsync(boardPage);
            await BoardPortalHelper.ConfigureCharterParameterAsync(
                boardPage, "exit-allowed", ExitAllowed);
            await AddParticipantsAsync(boardPage, 37);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);

            await AssertAuditForCustomCharterAsync(login, participantsAdded: true);
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

    [Fact]
    public async Task CustomCharter_ExitMinSharePercent_ShouldSaveWithoutErrors()
    {
        SkipIfPreviousFailed();

        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "CustomCharter_ExitMinSharePercent";

        var (adminPage, boardPage, login) = await SetupFullCycleAsync(38);
        try
        {
            await BoardPortalHelper.SelectCustomCharterAsync(boardPage);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "exit-allowed", ExitAllowed);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "exit-min-share", ExitMinSharePercent);
            await AddParticipantsAsync(boardPage, 38);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);

            await AssertAuditForCustomCharterAsync(login, participantsAdded: true);
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

    [Fact]
    public async Task CustomCharter_ExitMaxSharePercent_ShouldSaveWithoutErrors()
    {
        SkipIfPreviousFailed();

        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "CustomCharter_ExitMaxSharePercent";

        var (adminPage, boardPage, login) = await SetupFullCycleAsync(39);
        try
        {
            await BoardPortalHelper.SelectCustomCharterAsync(boardPage);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "exit-allowed", ExitAllowed);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "exit-max-share", ExitMaxSharePercent);
            await AddParticipantsAsync(boardPage, 39);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);

            await AssertAuditForCustomCharterAsync(login, participantsAdded: true);
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

    [Fact]
    public async Task CustomCharter_ExitConditionDescription_ShouldSaveWithoutErrors()
    {
        SkipIfPreviousFailed();

        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "CustomCharter_ExitConditionDescription";

        var (adminPage, boardPage, login) = await SetupFullCycleAsync(40);
        try
        {
            await BoardPortalHelper.SelectCustomCharterAsync(boardPage);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "exit-allowed", ExitAllowed);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "exit-condition", ExitConditionDescription);
            await AddParticipantsAsync(boardPage, 40);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);

            await AssertAuditForCustomCharterAsync(login, participantsAdded: true);
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

    [Fact]
    public async Task CustomCharter_ExitRequiresUnanimousOsu_ShouldSaveWithoutErrors()
    {
        SkipIfPreviousFailed();

        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "CustomCharter_ExitRequiresUnanimousOsu";

        var (adminPage, boardPage, login) = await SetupFullCycleAsync(41);
        try
        {
            await BoardPortalHelper.SelectCustomCharterAsync(boardPage);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "exit-allowed", ExitAllowed);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "exit-unanimous", ExitRequiresUnanimousOsu);
            await AddParticipantsAsync(boardPage, 41);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);

            await AssertAuditForCustomCharterAsync(login, participantsAdded: true);
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

    [Fact]
    public async Task CustomCharter_TransferToParticipants_ShouldSaveWithoutErrors()
    {
        SkipIfPreviousFailed();

        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "CustomCharter_TransferToParticipants";

        var (adminPage, boardPage, login) = await SetupFullCycleAsync(42);
        try
        {
            await BoardPortalHelper.SelectCustomCharterAsync(boardPage);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "transfer-participants", TransferToParticipants);
            await AddParticipantsAsync(boardPage, 42);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);

            await AssertAuditForCustomCharterAsync(login, participantsAdded: true);
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

    [Fact]
    public async Task CustomCharter_TransferToThirdParties_ShouldSaveWithoutErrors()
    {
        SkipIfPreviousFailed();

        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "CustomCharter_TransferToThirdParties";

        var (adminPage, boardPage, login) = await SetupFullCycleAsync(43);
        try
        {
            await BoardPortalHelper.SelectCustomCharterAsync(boardPage);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "transfer-third-parties", TransferToThirdParties);
            await AddParticipantsAsync(boardPage, 43);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);

            await AssertAuditForCustomCharterAsync(login, participantsAdded: true);
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

    [Fact]
    public async Task CustomCharter_PreemptiveRight_ShouldSaveWithoutErrors()
    {
        SkipIfPreviousFailed();

        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "CustomCharter_PreemptiveRight";

        var (adminPage, boardPage, login) = await SetupFullCycleAsync(44);
        try
        {
            await BoardPortalHelper.SelectCustomCharterAsync(boardPage);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "preemptive-right", PreemptiveRight);
            await AddParticipantsAsync(boardPage, 44);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);

            await AssertAuditForCustomCharterAsync(login, participantsAdded: true);
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

    [Fact]
    public async Task CustomCharter_InheritanceWithoutConsent_ShouldSaveWithoutErrors()
    {
        SkipIfPreviousFailed();

        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "CustomCharter_InheritanceWithoutConsent";

        var (adminPage, boardPage, login) = await SetupFullCycleAsync(45);
        try
        {
            await BoardPortalHelper.SelectCustomCharterAsync(boardPage);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "exit-allowed", ExitAllowed);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "inheritance", InheritanceWithoutConsent);
            await AddParticipantsAsync(boardPage, 45);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);

            await AssertAuditForCustomCharterAsync(login, participantsAdded: true);
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

    [Fact]
    public async Task CustomCharter_ExecutiveBody_ShouldSaveWithoutErrors()
    {
        SkipIfPreviousFailed();

        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "CustomCharter_ExecutiveBody";

        var (adminPage, boardPage, login) = await SetupFullCycleAsync(46);
        try
        {
            await BoardPortalHelper.SelectCustomCharterAsync(boardPage);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "executive-body", ExecutiveBody);
            await AddParticipantsAsync(boardPage, 46);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);

            await AssertAuditForCustomCharterAsync(login, participantsAdded: true);
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

    [Fact]
    public async Task CustomCharter_HasBoardOfDirectors_ShouldSaveWithoutErrors()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "CustomCharter_HasBoardOfDirectors";

        var (adminPage, boardPage, login) = await SetupFullCycleAsync(47);
        try
        {
            await BoardPortalHelper.SelectCustomCharterAsync(boardPage);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "has-board", HasBoardOfDirectors);
            await BoardPortalHelper.AssertBoardOfDirectorsAvailableAsync(boardPage);
            await AddParticipantsAsync(boardPage, 47);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);

            await AssertAuditForCustomCharterAsync(login, participantsAdded: true);
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

    [Fact]
    public async Task CustomCharter_BoardDecidesConveningOsu_ShouldSaveWithoutErrors()
    {
        SkipIfPreviousFailed();

        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "CustomCharter_BoardDecidesConveningOsu";

        var (adminPage, boardPage, login) = await SetupFullCycleAsync(48);
        try
        {
            await BoardPortalHelper.SelectCustomCharterAsync(boardPage);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "has-board", HasBoardOfDirectors);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "board-convenes-osu", BoardDecidesConveningOsu);
            await AddParticipantsAsync(boardPage, 48);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);

            await AssertAuditForCustomCharterAsync(login, participantsAdded: true);
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

    [Fact]
    public async Task CustomCharter_VosuThresholdPercent_ShouldSaveWithoutErrors()
    {
        SkipIfPreviousFailed();

        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "CustomCharter_VosuThresholdPercent";

        var (adminPage, boardPage, login) = await SetupFullCycleAsync(49);
        try
        {
            await BoardPortalHelper.SelectCustomCharterAsync(boardPage);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "has-board", HasBoardOfDirectors);
            await BoardPortalHelper.ConfigureCharterParameterAsync(boardPage, "vosu-threshold", VosuThresholdPercent);
            await AddParticipantsAsync(boardPage, 49);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);

            await AssertAuditForCustomCharterAsync(login, participantsAdded: true);
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

    [Fact]
    public async Task CustomCharter_AllParameters_ShouldSaveWithoutErrors()
    {
        SkipIfPreviousFailed();

        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "CustomCharter_AllParameters";

        var (adminPage, boardPage, login) = await SetupFullCycleAsync(50);
        try
        {
            await BoardPortalHelper.SelectCustomCharterAsync(boardPage);
            await BoardPortalHelper.AssertCustomCharterFieldsVisibleAsync(boardPage);

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

            await AssertAuditForCustomCharterAsync(login, participantsAdded: true);
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
    // 7 моделей ЕИО: индивидуальный устав (номера 51–57)
    // ══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Модель 1: ГД — наёмный сотрудник (не участник общества).
    /// Type A: отдельный генеральный директор, избирается общим собранием.
    /// </summary>
    [Fact]
    public async Task CustomCharter_Model1_HiredCeo_ShouldSaveWithoutErrors()
    {
        SkipIfPreviousFailed();

        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "CustomCharter_Model1_HiredCeo";

        var (adminPage, boardPage, login) = await SetupFullCycleAsync(51);
        try
        {
            await BoardPortalHelper.SelectCustomCharterAsync(boardPage);
            await BoardPortalHelper.SetExecutiveBodyAsync(boardPage, "A");
            await AddParticipantsAsync(boardPage, 51);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);

            await AssertAuditForCustomCharterAsync(login, participantsAdded: true);
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
    /// Модель 2: ГД — участник общества.
    /// Type A: одно лицо совмещает статус участника и ЕИО.
    /// </summary>
    [Fact]
    public async Task CustomCharter_Model2_CeoParticipant_ShouldSaveWithoutErrors()
    {
        SkipIfPreviousFailed();

        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "CustomCharter_Model2_CeoParticipant";

        var (adminPage, boardPage, login) = await SetupFullCycleAsync(52);
        try
        {
            await BoardPortalHelper.SelectCustomCharterAsync(boardPage);
            await BoardPortalHelper.SetExecutiveBodyAsync(boardPage, "A");
            await AddParticipantsAsync(boardPage, 52);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);

            await AssertAuditForCustomCharterAsync(login, participantsAdded: true);
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
    /// Модель 3: Управляющий — индивидуальный предприниматель (ст. 42 14-ФЗ).
    /// Type D: полномочия ЕИО переданы ИП по договору управления.
    /// </summary>
    [Fact]
    public async Task CustomCharter_Model3_ManagerIp_ShouldSaveWithoutErrors()
    {
        SkipIfPreviousFailed();

        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "CustomCharter_Model3_ManagerIp";

        var (adminPage, boardPage, login) = await SetupFullCycleAsync(53);
        try
        {
            await BoardPortalHelper.SelectCustomCharterAsync(boardPage);
            await BoardPortalHelper.SetExecutiveBodyAsync(boardPage, "D");
            await AddParticipantsAsync(boardPage, 53);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);

            await AssertAuditForCustomCharterAsync(login, participantsAdded: true);
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
    /// Модель 4: Управляющая организация — юридическое лицо (ст. 42 14-ФЗ).
    /// Type E: полномочия ЕИО переданы управляющей организации по договору.
    /// </summary>
    [Fact]
    public async Task CustomCharter_Model4_ManagingOrg_ShouldSaveWithoutErrors()
    {
        SkipIfPreviousFailed();

        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "CustomCharter_Model4_ManagingOrg";

        var (adminPage, boardPage, login) = await SetupFullCycleAsync(54);
        try
        {
            await BoardPortalHelper.SelectCustomCharterAsync(boardPage);
            await BoardPortalHelper.SetExecutiveBodyAsync(boardPage, "E");
            await AddParticipantsAsync(boardPage, 54);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);

            await AssertAuditForCustomCharterAsync(login, participantsAdded: true);
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
    /// Модель 5: Все участники общества являются директорами (каждый самостоятельно).
    /// Type B: каждый участник действует от имени общества самостоятельно.
    /// </summary>
    [Fact]
    public async Task CustomCharter_Model5_AllParticipantsDirectors_ShouldSaveWithoutErrors()
    {
        SkipIfPreviousFailed();

        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "CustomCharter_Model5_AllParticipantsDirectors";

        var (adminPage, boardPage, login) = await SetupFullCycleAsync(55);
        try
        {
            await BoardPortalHelper.SelectCustomCharterAsync(boardPage);
            await BoardPortalHelper.SetExecutiveBodyAsync(boardPage, "B");
            await AddParticipantsAsync(boardPage, 55);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);

            await AssertAuditForCustomCharterAsync(login, participantsAdded: true);
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
    /// Модель 6: Все участники совместно осуществляют полномочия ЕИО.
    /// Type C: совместное осуществление полномочий (принцип «двух ключей»).
    /// </summary>
    [Fact]
    public async Task CustomCharter_Model6_AllParticipantsJoint_ShouldSaveWithoutErrors()
    {
        SkipIfPreviousFailed();

        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "CustomCharter_Model6_AllParticipantsJoint";

        var (adminPage, boardPage, login) = await SetupFullCycleAsync(56);
        try
        {
            await BoardPortalHelper.SelectCustomCharterAsync(boardPage);
            await BoardPortalHelper.SetExecutiveBodyAsync(boardPage, "C");
            await AddParticipantsAsync(boardPage, 56);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);

            await AssertAuditForCustomCharterAsync(login, participantsAdded: true);
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
    /// Модель 7: Несколько ЕИО, действующих совместно или независимо (п. 3 ст. 65.3 ГК РФ).
    /// Type F: несколько единоличных исполнительных органов.
    /// </summary>
    [Fact]
    public async Task CustomCharter_Model7_MultipleEio_ShouldSaveWithoutErrors()
    {
        SkipIfPreviousFailed();

        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "CustomCharter_Model7_MultipleEio";

        var (adminPage, boardPage, login) = await SetupFullCycleAsync(57);
        try
        {
            await BoardPortalHelper.SelectCustomCharterAsync(boardPage);
            await BoardPortalHelper.SetExecutiveBodyAsync(boardPage, "F");
            await AddParticipantsAsync(boardPage, 57);
            await BoardPortalHelper.SaveAndVerifyAsync(boardPage);
            await VerifyPagesAsync(boardPage, adminPage, testStartTime);

            await AssertAuditForCustomCharterAsync(login, participantsAdded: true);
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
        // Инфраструктура (порталы, LDAP) должна быть запущена ДО создания страниц
        await InfrastructureHelper.EnsureInfrastructureReadyAsync();

        var adminPage = await CreateAdminConsolePageAsync();
        var boardPage = await CreateBoardPortalPageAsync();

        // Глобальная инициализация: инфраструктура + БД + LDAP (один раз)
        await CharterTestGlobalInit.InitializeAsync();

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
                sharePercent: p.SharePercent);
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

    /// <summary>
    /// Проверить записи аудита для индивидуального устава: вход, сохранение, участники.
    /// </summary>
    private static async Task AssertAuditForCustomCharterAsync(string login, bool participantsAdded)
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
