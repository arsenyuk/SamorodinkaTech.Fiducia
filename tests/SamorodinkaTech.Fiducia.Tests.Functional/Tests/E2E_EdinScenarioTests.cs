using FluentAssertions;
using Microsoft.Playwright;
using SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

namespace SamorodinkaTech.Fiducia.Tests.Functional;

/// <summary>
/// E2E-тесты: сквозные сценарии ЕДИН-интеграции для ООО с ЕИО-ГД.
/// Требуют запущенных порталов, ЕДИН и Playwright.
/// </summary>
public class E2E_EdinScenarioTests : BrowserFixture
{
    public E2E_EdinScenarioTests(GlobalFixture globalFixture) : base(globalFixture)
    {
    }

    /// <summary>
    /// Сценарий 1: SYS_ADMIN создаёт ЮЛ → назначает Администратора ЮЛ →
    /// LE_ADMIN вводит ПДн для ГД → ЕДИН привязывает MasterId → роль CEO.
    /// </summary>
    [Fact]
    public async Task Scenario1_AdminCreatesLeAndBindsEdin()
    {
        var adminPage = await CreateAdminConsolePageAsync("/login");
        var boardPage = await CreateBoardPortalPageAsync("/login");

        try
        {
            // ── Шаг 1: SYS_ADMIN логинится ──────────────────────────────
            await AuthHelper.LoginAsAdminAsync(adminPage, "v.vasilyeva", "1");
            await adminPage.ScreenshotAsync(new() { Path = "/tmp/e2e-after-login.png" });
            Console.WriteLine($"[DEBUG] After login URL: {adminPage.Url}");

            // ── Шаг 2: Создать ЮЛ (ООО, ОКОПФ 12300) ──────────────────
            var leName = $"ООО «ЕДИН Тест {DateTime.UtcNow:yyyyMMddHHmmss}»";
            var leInn = InnTestHelper.GenerateValidInn();
            await AdminConsoleHelper.CreateLegalEntityAsync(adminPage, leName, leInn);

            // Установить ОКОПФ = 12300 (ООО)
            var selectedLeId = await adminPage.EvaluateAsync<string?>(
                @"() => {
                    const sel = document.querySelector('.card-body select.form-select');
                    return sel ? sel.value : null;
                }");
            if (!string.IsNullOrEmpty(selectedLeId) && Guid.TryParse(selectedLeId, out var leGuid))
            {
                await AdminConsoleHelper.SetOkopfAsync(adminPage, leGuid, "12300");
            }

            // ── Шаг 3: Создать пользователя в БД (LDAP-аутентификация: Basic) ──
            var adminLogin = $"admin_{DateTime.UtcNow:HHmmss}";
            await AdminConsoleHelper.CreateUserViaUiAsync(
                adminPage, adminLogin,
                "Смирнов", "Алексей", "Петрович",
                $"{adminLogin}@test.local");

            // ── Шаг 4: Назначить Администратора ЮЛ (LE_ADMIN) ──────────
            await AdminConsoleHelper.AddEmployeeAsync(
                adminPage,
                "Смирнов", "Алексей", "Петрович",
                "Администратор ЮЛ",
                adminLogin,
                "LE_ADMIN");

            // ── Шаг 4: LE_ADMIN логинится в Board Portal ────────────────
            await AuthHelper.LoginAsBoardUserAsync(boardPage, adminLogin, "1");

            // ── Шаг 5: LE_ADMIN вводит ПДн для ГД (участник с паспортом) ──
            var participantId = await BoardPortalHelper.AddParticipantWithPersonalDataAsync(
                boardPage,
                fullName: "Смирнов Алексей Петрович",
                passportSeries: "4515",
                passportNumber: "111222",
                personInn: "770888999000",
                participantType: "FL",
                sharePercent: 100m,
                shareAmount: 10000m);

            participantId.Should().NotBeEmpty("участник должен быть создан");

            // ── Шаг 6: Ожидание ЕДИН binding ────────────────────────────
            // (fire-and-forget: TriggerEdinBindingAsync)
            await EdinTestHelper.WaitForEdinBindingAsync(boardPage, participantId, timeoutSeconds: 15);

            var mpiMasterId = await EdinTestHelper.GetParticipantMpiMasterIdAsync(boardPage, participantId);
            mpiMasterId.Should().NotBeNull("ЕДИН должен привязать MasterId");

            // ── Шаг 7: SYS_ADMIN назначает роль CEO ─────────────────────
            // (нужен ID пользователя — для этого ищем по логину)
            // Используем AccessManagement для добавления роли CEO
            await AuthHelper.LoginAsAdminAsync(adminPage, "v.vasilyeva", "1");
            await EdinTestHelper.AssignRoleViaAccessManagementAsync(
                adminPage,
                "Смирнов", "Алексей", "Петрович",
                "Генеральный директор",
                adminLogin,
                "CEO");

            // ── Проверки ─────────────────────────────────────────────────
            // ЮЛ создано и доступно
            var content = await adminPage.ContentAsync();
            content.Should().Contain(leName, "ЮЛ должно отображаться в списке");

            Console.WriteLine($"[Scenario1] ЮЛ: {leName}, LE_ADMIN: {adminLogin}, MPI: {mpiMasterId}");
        }
        finally
        {
            await adminPage.CloseAsync();
            await boardPage.CloseAsync();
        }
    }

    /// <summary>
    /// Сценарий 2: Дедупликация через ЕДИН — ГД вводит участника с теми же ПДн.
    /// MasterId совпадает → user_id участника = userId ГД → роль PARTICIPANT.
    /// </summary>
    [Fact]
    public async Task Scenario2_DeduplicationViaEdin()
    {
        var adminPage = await CreateAdminConsolePageAsync("/login");
        var boardPage = await CreateBoardPortalPageAsync("/login");

        try
        {
            // ── Шаги 1–6: Setup (из сценария 1) ─────────────────────────
            await AuthHelper.LoginAsAdminAsync(adminPage, "v.vasilyeva", "1");

            var leName = $"ООО «ЕДИН Дедуп {DateTime.UtcNow:yyyyMMddHHmmss}»";
            var leInn = InnTestHelper.GenerateValidInn();
            await AdminConsoleHelper.CreateLegalEntityAsync(adminPage, leName, leInn);

            var adminLogin = $"dedup_{DateTime.UtcNow:HHmmss}";
            await AdminConsoleHelper.CreateUserViaUiAsync(
                adminPage, adminLogin,
                "Петрова", "Мария", "Сергеевна",
                $"{adminLogin}@test.local");

            await AdminConsoleHelper.AddEmployeeAsync(
                adminPage,
                "Петрова", "Мария", "Сергеевна",
                "Администратор ЮЛ",
                adminLogin,
                "LE_ADMIN");

            await AuthHelper.LoginAsBoardUserAsync(boardPage, adminLogin, "1");

            // Добавляем ГД как участника с ПДн
            var participantId1 = await BoardPortalHelper.AddParticipantWithPersonalDataAsync(
                boardPage,
                fullName: "Петрова Мария Сергеевна",
                passportSeries: "4516",
                passportNumber: "222333",
                personInn: "770999111000",
                participantType: "FL",
                sharePercent: 100m,
                shareAmount: 10000m);

            await EdinTestHelper.WaitForEdinBindingAsync(boardPage, participantId1, timeoutSeconds: 15);
            var masterId1 = await EdinTestHelper.GetParticipantMpiMasterIdAsync(boardPage, participantId1);
            masterId1.Should().NotBeNull("Первый ЕДИН binding должен завершиться");

            // ── Шаг 7: Добавляем участника с ТЕМИ ЖЕ ПДн ────────────────
            var participantId2 = await BoardPortalHelper.AddParticipantWithPersonalDataAsync(
                boardPage,
                fullName: "Петрова Мария Сергеевна",
                passportSeries: "4516",
                passportNumber: "222333",
                personInn: "770999111000",
                participantType: "FL",
                sharePercent: 0m,
                shareAmount: 0m);

            await EdinTestHelper.WaitForEdinBindingAsync(boardPage, participantId2, timeoutSeconds: 15);

            // ── Шаг 8: Проверить, что MasterId совпадает ────────────────
            var masterId2 = await EdinTestHelper.GetParticipantMpiMasterIdAsync(boardPage, participantId2);
            masterId2.Should().Be(masterId1, "Те же ПДн → тот же MasterId");

            // ── Шаг 9: SYS_ADMIN назначает роль PARTICIPANT ─────────────
            await AuthHelper.LoginAsAdminAsync(adminPage, "v.vasilyeva", "1");
            await EdinTestHelper.AssignRoleViaAccessManagementAsync(
                adminPage,
                "Петрова", "Мария", "Сергеевна",
                "Участник",
                adminLogin,
                "PARTICIPANT");

            // ── Проверки ─────────────────────────────────────────────────
            masterId1.Should().NotBeNull();
            masterId2.Should().Be(masterId1);

            Console.WriteLine($"[Scenario2] Мастер 1: {masterId1}, Мастер 2: {masterId2}");
        }
        finally
        {
            await adminPage.CloseAsync();
            await boardPage.CloseAsync();
        }
    }

}
