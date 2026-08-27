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
        // US-004: Пользователи
        await VerifyPageAsync(adminPage, "/users",
            "_framework/blazor.server.js", "Admin Console: Users");

        // US-004: Роли
        await VerifyPageAsync(adminPage, "/roles",
            "_framework/blazor.server.js", "Admin Console: Roles");

        // US-004: Шаблоны организаций
        await VerifyPageAsync(adminPage, "/org-templates",
            "_framework/blazor.server.js", "Admin Console: OrgTemplates");

        // US-004: Отправленные уведомления
        await VerifyPageAsync(adminPage, "/sent-notifications",
            "_framework/blazor.server.js", "Admin Console: SentNotifications");

        // US-004: Справочники
        await VerifyPageAsync(adminPage, "/dictionaries",
            "_framework/blazor.server.js", "Admin Console: Dictionaries");

        // US-002: Настройки
        await VerifyPageAsync(adminPage, "/settings",
            "_framework/blazor.server.js", "Admin Console: Settings");

        // US-002: Аудит
        await VerifyPageAsync(adminPage, "/audit",
            "_framework/blazor.server.js", "Admin Console: Audit");

        // US-002: ОСА
        await VerifyPageAsync(adminPage, "/osa-meetings",
            "_framework/blazor.server.js", "Admin Console: OsaMeetings");
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
