using FluentAssertions;
using Microsoft.Playwright;
using SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

namespace SamorodinkaTech.Fiducia.Tests.Functional;

/// <summary>
/// Сквозные E2E-тесты для 36 типовых уставов ООО.
/// БД сбрасывается ОДИН раз перед прогоном всех тестов.
/// Каждый тест работает со своим фиксированным ЮЛ и набором лиц.
/// Запрещено параллельное исполнение (Collection "CharterTests").
/// Тестовый сценарий:
/// 1. Логин ГД в Board Portal (пользователь уже создан при сидировании)
/// 2. Заполнение полей ЮЛ + выбор типового устава
/// 3. Сохранение и проверка отсутствия ошибок
/// 4. Добавление участников (для ExecutiveBody A)
/// 5. Проверка страниц Board Portal и Admin Console (US-002..US-024)
/// 6. Проверка записей аудита (вход, чтение/запись, участники)
/// 7. Проверка отсутствия ошибок в логе приложения за период работы теста
/// </summary>
[Collection("CharterTests")]
public class E2E_StandardCharterTests : BrowserFixture
{
    // ══════════════════════════════════════════════════════════════════════
    // Тесты: 36 самостоятельных [Fact] (по одному на каждый типовой устав)
    // ══════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task StandardCharter01_CompleteFlow()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "StandardCharter01_CompleteFlow";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(1);
        try
        {
            await ExecuteCharterFlowAsync(boardPage, adminPage, charterNumber: 1, testStartTime);
            await AssertAuditAsync(login, entityHasExecutiveBodyA: true);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task StandardCharter02_CompleteFlow()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "StandardCharter02_CompleteFlow";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(2);
        try
        {
            await ExecuteCharterFlowAsync(boardPage, adminPage, charterNumber: 2, testStartTime);
            await AssertAuditAsync(login, entityHasExecutiveBodyA: true);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task StandardCharter03_CompleteFlow()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "StandardCharter03_CompleteFlow";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(3);
        try
        {
            await ExecuteCharterFlowAsync(boardPage, adminPage, charterNumber: 3, testStartTime);
            await AssertAuditAsync(login, entityHasExecutiveBodyA: true);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task StandardCharter04_CompleteFlow()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "StandardCharter04_CompleteFlow";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(4);
        try
        {
            await ExecuteCharterFlowAsync(boardPage, adminPage, charterNumber: 4, testStartTime);
            await AssertAuditAsync(login, entityHasExecutiveBodyA: true);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task StandardCharter05_CompleteFlow()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "StandardCharter05_CompleteFlow";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(5);
        try
        {
            await ExecuteCharterFlowAsync(boardPage, adminPage, charterNumber: 5, testStartTime);
            await AssertAuditAsync(login, entityHasExecutiveBodyA: true);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task StandardCharter06_CompleteFlow()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "StandardCharter06_CompleteFlow";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(6);
        try
        {
            await ExecuteCharterFlowAsync(boardPage, adminPage, charterNumber: 6, testStartTime);
            await AssertAuditAsync(login, entityHasExecutiveBodyA: true);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task StandardCharter07_CompleteFlow()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "StandardCharter07_CompleteFlow";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(7);
        try
        {
            await ExecuteCharterFlowAsync(boardPage, adminPage, charterNumber: 7, testStartTime);
            await AssertAuditAsync(login, entityHasExecutiveBodyA: false);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task StandardCharter08_CompleteFlow()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "StandardCharter08_CompleteFlow";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(8);
        try
        {
            await ExecuteCharterFlowAsync(boardPage, adminPage, charterNumber: 8, testStartTime);
            await AssertAuditAsync(login, entityHasExecutiveBodyA: false);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task StandardCharter09_CompleteFlow()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "StandardCharter09_CompleteFlow";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(9);
        try
        {
            await ExecuteCharterFlowAsync(boardPage, adminPage, charterNumber: 9, testStartTime);
            await AssertAuditAsync(login, entityHasExecutiveBodyA: false);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task StandardCharter10_CompleteFlow()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "StandardCharter10_CompleteFlow";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(10);
        try
        {
            await ExecuteCharterFlowAsync(boardPage, adminPage, charterNumber: 10, testStartTime);
            await AssertAuditAsync(login, entityHasExecutiveBodyA: false);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task StandardCharter11_CompleteFlow()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "StandardCharter11_CompleteFlow";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(11);
        try
        {
            await ExecuteCharterFlowAsync(boardPage, adminPage, charterNumber: 11, testStartTime);
            await AssertAuditAsync(login, entityHasExecutiveBodyA: false);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task StandardCharter12_CompleteFlow()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "StandardCharter12_CompleteFlow";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(12);
        try
        {
            await ExecuteCharterFlowAsync(boardPage, adminPage, charterNumber: 12, testStartTime);
            await AssertAuditAsync(login, entityHasExecutiveBodyA: false);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task StandardCharter13_CompleteFlow()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "StandardCharter13_CompleteFlow";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(13);
        try
        {
            await ExecuteCharterFlowAsync(boardPage, adminPage, charterNumber: 13, testStartTime);
            await AssertAuditAsync(login, entityHasExecutiveBodyA: false);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task StandardCharter14_CompleteFlow()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "StandardCharter14_CompleteFlow";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(14);
        try
        {
            await ExecuteCharterFlowAsync(boardPage, adminPage, charterNumber: 14, testStartTime);
            await AssertAuditAsync(login, entityHasExecutiveBodyA: false);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task StandardCharter15_CompleteFlow()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "StandardCharter15_CompleteFlow";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(15);
        try
        {
            await ExecuteCharterFlowAsync(boardPage, adminPage, charterNumber: 15, testStartTime);
            await AssertAuditAsync(login, entityHasExecutiveBodyA: false);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task StandardCharter16_CompleteFlow()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "StandardCharter16_CompleteFlow";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(16);
        try
        {
            await ExecuteCharterFlowAsync(boardPage, adminPage, charterNumber: 16, testStartTime);
            await AssertAuditAsync(login, entityHasExecutiveBodyA: false);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task StandardCharter17_CompleteFlow()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "StandardCharter17_CompleteFlow";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(17);
        try
        {
            await ExecuteCharterFlowAsync(boardPage, adminPage, charterNumber: 17, testStartTime);
            await AssertAuditAsync(login, entityHasExecutiveBodyA: false);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task StandardCharter18_CompleteFlow()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "StandardCharter18_CompleteFlow";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(18);
        try
        {
            await ExecuteCharterFlowAsync(boardPage, adminPage, charterNumber: 18, testStartTime);
            await AssertAuditAsync(login, entityHasExecutiveBodyA: false);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task StandardCharter19_CompleteFlow()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "StandardCharter19_CompleteFlow";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(19);
        try
        {
            await ExecuteCharterFlowAsync(boardPage, adminPage, charterNumber: 19, testStartTime);
            await AssertAuditAsync(login, entityHasExecutiveBodyA: true);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task StandardCharter20_CompleteFlow()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "StandardCharter20_CompleteFlow";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(20);
        try
        {
            await ExecuteCharterFlowAsync(boardPage, adminPage, charterNumber: 20, testStartTime);
            await AssertAuditAsync(login, entityHasExecutiveBodyA: true);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task StandardCharter21_CompleteFlow()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "StandardCharter21_CompleteFlow";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(21);
        try
        {
            await ExecuteCharterFlowAsync(boardPage, adminPage, charterNumber: 21, testStartTime);
            await AssertAuditAsync(login, entityHasExecutiveBodyA: true);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task StandardCharter22_CompleteFlow()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "StandardCharter22_CompleteFlow";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(22);
        try
        {
            await ExecuteCharterFlowAsync(boardPage, adminPage, charterNumber: 22, testStartTime);
            await AssertAuditAsync(login, entityHasExecutiveBodyA: true);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task StandardCharter23_CompleteFlow()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "StandardCharter23_CompleteFlow";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(23);
        try
        {
            await ExecuteCharterFlowAsync(boardPage, adminPage, charterNumber: 23, testStartTime);
            await AssertAuditAsync(login, entityHasExecutiveBodyA: true);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task StandardCharter24_CompleteFlow()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "StandardCharter24_CompleteFlow";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(24);
        try
        {
            await ExecuteCharterFlowAsync(boardPage, adminPage, charterNumber: 24, testStartTime);
            await AssertAuditAsync(login, entityHasExecutiveBodyA: true);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task StandardCharter25_CompleteFlow()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "StandardCharter25_CompleteFlow";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(25);
        try
        {
            await ExecuteCharterFlowAsync(boardPage, adminPage, charterNumber: 25, testStartTime);
            await AssertAuditAsync(login, entityHasExecutiveBodyA: false);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task StandardCharter26_CompleteFlow()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "StandardCharter26_CompleteFlow";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(26);
        try
        {
            await ExecuteCharterFlowAsync(boardPage, adminPage, charterNumber: 26, testStartTime);
            await AssertAuditAsync(login, entityHasExecutiveBodyA: false);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task StandardCharter27_CompleteFlow()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "StandardCharter27_CompleteFlow";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(27);
        try
        {
            await ExecuteCharterFlowAsync(boardPage, adminPage, charterNumber: 27, testStartTime);
            await AssertAuditAsync(login, entityHasExecutiveBodyA: false);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task StandardCharter28_CompleteFlow()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "StandardCharter28_CompleteFlow";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(28);
        try
        {
            await ExecuteCharterFlowAsync(boardPage, adminPage, charterNumber: 28, testStartTime);
            await AssertAuditAsync(login, entityHasExecutiveBodyA: false);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task StandardCharter29_CompleteFlow()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "StandardCharter29_CompleteFlow";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(29);
        try
        {
            await ExecuteCharterFlowAsync(boardPage, adminPage, charterNumber: 29, testStartTime);
            await AssertAuditAsync(login, entityHasExecutiveBodyA: false);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task StandardCharter30_CompleteFlow()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "StandardCharter30_CompleteFlow";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(30);
        try
        {
            await ExecuteCharterFlowAsync(boardPage, adminPage, charterNumber: 30, testStartTime);
            await AssertAuditAsync(login, entityHasExecutiveBodyA: false);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task StandardCharter31_CompleteFlow()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "StandardCharter31_CompleteFlow";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(31);
        try
        {
            await ExecuteCharterFlowAsync(boardPage, adminPage, charterNumber: 31, testStartTime);
            await AssertAuditAsync(login, entityHasExecutiveBodyA: false);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task StandardCharter32_CompleteFlow()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "StandardCharter32_CompleteFlow";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(32);
        try
        {
            await ExecuteCharterFlowAsync(boardPage, adminPage, charterNumber: 32, testStartTime);
            await AssertAuditAsync(login, entityHasExecutiveBodyA: false);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task StandardCharter33_CompleteFlow()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "StandardCharter33_CompleteFlow";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(33);
        try
        {
            await ExecuteCharterFlowAsync(boardPage, adminPage, charterNumber: 33, testStartTime);
            await AssertAuditAsync(login, entityHasExecutiveBodyA: false);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task StandardCharter34_CompleteFlow()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "StandardCharter34_CompleteFlow";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(34);
        try
        {
            await ExecuteCharterFlowAsync(boardPage, adminPage, charterNumber: 34, testStartTime);
            await AssertAuditAsync(login, entityHasExecutiveBodyA: false);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task StandardCharter35_CompleteFlow()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "StandardCharter35_CompleteFlow";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(35);
        try
        {
            await ExecuteCharterFlowAsync(boardPage, adminPage, charterNumber: 35, testStartTime);
            await AssertAuditAsync(login, entityHasExecutiveBodyA: false);
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    [Fact]
    public async Task StandardCharter36_CompleteFlow()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "StandardCharter36_CompleteFlow";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(36);
        try
        {
            await ExecuteCharterFlowAsync(boardPage, adminPage, charterNumber: 36, testStartTime);
            await AssertAuditAsync(login, entityHasExecutiveBodyA: false);
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
        await CharterTestSeeder.EnsureSeededAsync(adminPage, ldapPage);

        // Получение данных для данного ЮЛ
        var persons = CharterTestDataFixed.PersonsByEntity[charterNumber];

        // Логин ГД в Board Portal
        var gdLogin = persons.Gd?.Uid ?? persons.Participants[0].Uid;
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

    private static async Task CleanupAsync(IPage adminPage, IPage boardPage, IPage ldapPage)
    {
        await ldapPage.CloseAsync();
        await boardPage.CloseAsync();
        await adminPage.CloseAsync();
    }
}
