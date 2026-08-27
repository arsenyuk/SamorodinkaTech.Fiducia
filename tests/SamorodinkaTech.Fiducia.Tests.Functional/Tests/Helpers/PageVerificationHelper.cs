using FluentAssertions;
using Microsoft.Playwright;

namespace SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

/// <summary>
/// Проверка рендеринга страниц Board Portal и Admin Console
/// после настройки устава и добавления участников.
/// После каждой страницы проверяется запись в логе аудита.
/// Используется как продолжение flow в E2E_StandardCharterTests / E2E_NonStandardCharterTests.
/// </summary>
public static class PageVerificationHelper
{
    private const int NetworkIdleTimeoutMs = 10_000;

    /// <summary>Задержка перед проверкой аудита (мс) — серверу нужно время на запись.</summary>
    private const int AuditWriteDelayMs = 1_000;

    /// <summary>
    /// Проверить основные страницы Board Portal (с авторизованной сессией).
    /// </summary>
    public static async Task VerifyBoardPortalPagesAsync(IPage boardPage)
    {
        // US-002: Заседания СД
        await VerifyPageAsync(boardPage, "/meetings",
            "Созывы заседаний СД", "Board Portal: Meetings");

        // US-002: Голосование (GUID-заглушка — ожидаем spinner или сообщение)
        await VerifyPageAsync(boardPage, "/voting/00000000-0000-0000-0000-000000000000",
            "spinner-border", "Board Portal: Voting");

        // US-004: Комитеты
        await VerifyPageAsync(boardPage, "/committees",
            "Комитеты совета директоров", "Board Portal: Committees");

        // US-004: Документы
        await VerifyPageAsync(boardPage, "/documents",
            "_framework/blazor.server.js", "Board Portal: Documents");

        // US-004: Печатные формы
        await VerifyPageAsync(boardPage, "/print-forms",
            "_framework/blazor.server.js", "Board Portal: PrintForms");

        // US-021: Каталог документов
        await VerifyPageAsync(boardPage, "/documents/catalog",
            "Предоставленные документы", "Board Portal: DocumentsCatalog");

        // US-022: ОСУ
        await VerifyPageAsync(boardPage, "/osu-meetings",
            "Общие собрания", "Board Portal: OsuMeetings");

        // US-022: Повестка ОСУ
        await VerifyPageAsync(boardPage, "/agenda-osu",
            "Повестка", "Board Portal: AgendaOsu");

        // US-023: Участники
        await VerifyPageAsync(boardPage, "/participants",
            "Участники", "Board Portal: Participants");

        // US-024: Договоры
        await VerifyPageAsync(boardPage, "/contracts",
            "Договоры", "Board Portal: Contracts");

        // US-010: Оповещения
        await VerifyPageAsync(boardPage, "/notifications",
            "Оповещения", "Board Portal: Notifications");
    }

    /// <summary>
    /// Проверить основные страницы Admin Console (с авторизованной сессией ГД → Admin).
    /// </summary>
    public static async Task VerifyAdminConsolePagesAsync(IPage adminPage)
    {
        // Основные страницы
        await VerifyPageAsync(adminPage, "/main",
            "_framework/blazor.server.js", "Admin Console: Main");

        await VerifyPageAsync(adminPage, "/users",
            "_framework/blazor.server.js", "Admin Console: Users");

        await VerifyPageAsync(adminPage, "/roles",
            "_framework/blazor.server.js", "Admin Console: Roles");

        await VerifyPageAsync(adminPage, "/settings",
            "_framework/blazor.server.js", "Admin Console: Settings");

        await VerifyPageAsync(adminPage, "/dictionaries",
            "_framework/blazor.server.js", "Admin Console: Dictionaries");

        // Управление доступом
        await VerifyPageAsync(adminPage, "/access-management",
            "_framework/blazor.server.js", "Admin Console: AccessManagement");

        // Юридические лица и уставы
        await VerifyPageAsync(adminPage, "/standard-charters",
            "_framework/blazor.server.js", "Admin Console: StandardCharters");

        await VerifyPageAsync(adminPage, "/board-of-directors-list",
            "_framework/blazor.server.js", "Admin Console: Boards");

        await VerifyPageAsync(adminPage, "/board-of-directors-statuses",
            "_framework/blazor.server.js", "Admin Console: BoardStatuses");

        await VerifyPageAsync(adminPage, "/board-member-appointment-statuses",
            "_framework/blazor.server.js", "Admin Console: AppointmentStatuses");

        await VerifyPageAsync(adminPage, "/resignation-reasons",
            "_framework/blazor.server.js", "Admin Console: ResignationReasons");

        await VerifyPageAsync(adminPage, "/dul-types",
            "_framework/blazor.server.js", "Admin Console: DulTypes");

        await VerifyPageAsync(adminPage, "/gd-terms",
            "_framework/blazor.server.js", "Admin Console: GdTerms");

        // Шаблоны организаций
        await VerifyPageAsync(adminPage, "/org-templates",
            "_framework/blazor.server.js", "Admin Console: OrgTemplates");

        // Уведомления
        await VerifyPageAsync(adminPage, "/notification-templates",
            "_framework/blazor.server.js", "Admin Console: NotificationTemplates");

        await VerifyPageAsync(adminPage, "/notification-types",
            "_framework/blazor.server.js", "Admin Console: NotificationTypes");

        await VerifyPageAsync(adminPage, "/sent-notifications",
            "_framework/blazor.server.js", "Admin Console: SentNotifications");

        // Измерения
        await VerifyPageAsync(adminPage, "/measurement-units",
            "_framework/blazor.server.js", "Admin Console: MeasurementUnits");

        // Email
        await VerifyPageAsync(adminPage, "/email-settings",
            "_framework/blazor.server.js", "Admin Console: EmailSettings");

        // Законодательство
        await VerifyPageAsync(adminPage, "/board-law",
            "_framework/blazor.server.js", "Admin Console: BoardLaw");
    }

    private static async Task VerifyPageAsync(IPage page, string path, string expectedText, string label)
    {
        var portal = page.Url.Contains("5001") ? Portal.AdminConsole : Portal.BoardPortal;

        await page.GotoAsync(PortalUrls.GetUrl(portal, path));
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle,
            new() { Timeout = NetworkIdleTimeoutMs });

        var content = await page.ContentAsync();

        if (expectedText == "_framework/blazor.server.js")
        {
            content.Should().Contain(expectedText, $"{label}: Blazor shell must load on {path}");
        }
        else
        {
            content.Should().Contain(expectedText,
                $"{label}: страница {path} должна содержать «{expectedText}»");
        }

        // Проверка записи в логе аудита после каждого перехода на страницу
        await page.WaitForTimeoutAsync(AuditWriteDelayMs);
        await AuditLogHelper.AssertPageAccessLoggedAsync(path);
    }
}
