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

    /// <summary>Время начала проверки страниц (для фильтрации аудита).</summary>
    private static DateTimeOffset _verifyStartTime;

    /// <summary>
    /// Проверить основные страницы Board Portal (с авторизованной сессией).
    /// </summary>
    public static async Task VerifyBoardPortalPagesAsync(IPage boardPage, DateTimeOffset? testStartTime = null)
    {
        _verifyStartTime = testStartTime ?? DateTimeOffset.UtcNow;
        // US-002: Заседания СД — заголовок + кнопка создания
        await VerifyPageAsync(boardPage, "/meetings",
            "Созывы заседаний СД", "Board Portal: Meetings");
        await VerifyButtonAsync(boardPage, "/meetings",
            "button.btn-primary", "Создать уведомление", "Board Portal: Meetings");

        // US-002: Голосование (GUID-заглушка — ожидаем spinner или сообщение)
        await VerifyPageAsync(boardPage, "/voting/00000000-0000-0000-0000-000000000000",
            "spinner-border", "Board Portal: Voting");

        // US-004: Комитеты — заголовок + типы поведения
        await VerifyPageAsync(boardPage, "/committees",
            "Комитеты совета директоров", "Board Portal: Committees");
        await VerifyContentAnyAsync(boardPage, "/committees",
            new[] { "Защитный", "Стратегический" }, "Board Portal: Committees");

        // US-004: Документы
        await VerifyPageAsync(boardPage, "/documents",
            "_framework/blazor.server.js", "Board Portal: Documents");

        // US-004: Печатные формы
        await VerifyPageAsync(boardPage, "/print-forms",
            "_framework/blazor.server.js", "Board Portal: PrintForms");

        // US-021: Каталог документов — заголовок + accordion-структура
        await VerifyPageAsync(boardPage, "/documents/catalog",
            "Предоставленные документы", "Board Portal: DocumentsCatalog");
        await VerifyContentAnyAsync(boardPage, "/documents/catalog",
            new[] { "accordion", "Нет предоставленных", "Юридическое лицо не выбрано" },
            "Board Portal: DocumentsCatalog");

        // US-022: ОСУ
        await VerifyPageAsync(boardPage, "/osu-meetings",
            "Общие собрания", "Board Portal: OsuMeetings");

        // US-022: Повестка ОСУ
        await VerifyPageAsync(boardPage, "/agenda-osu",
            "Повестка", "Board Portal: AgendaOsu");

        // US-023: Участники — заголовок + container-fluid
        await VerifyPageAsync(boardPage, "/participants",
            "Участники", "Board Portal: Participants");
        await VerifyVisibleAsync(boardPage, "/participants",
            ".container-fluid", "Board Portal: Participants");

        // US-024: Договоры
        await VerifyPageAsync(boardPage, "/contracts",
            "Договоры", "Board Portal: Contracts");

        // US-020: Требования (share requests)
        await VerifyContentAnyAsync(boardPage, "/share-requests",
            new[] { "Мои запросы", "Требования", "Нет запросов", "Подать требование" },
            "Board Portal: ShareRequests");

        // US-010: Оповещения
        await VerifyPageAsync(boardPage, "/notifications",
            "Оповещения", "Board Portal: Notifications");
    }

    /// <summary>
    /// Проверить основные страницы Admin Console (с авторизованной сессией ГД → Admin).
    /// </summary>
    public static async Task VerifyAdminConsolePagesAsync(IPage adminPage, DateTimeOffset? testStartTime = null)
    {
        _verifyStartTime = testStartTime ?? DateTimeOffset.UtcNow;

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

    private static async Task NavigateToPageAsync(IPage page, string path)
    {
        // Навигация через ссылку в меню (href без ведущего /)
        var href = path.TrimStart('/');
        var link = page.Locator($"a[href='{href}']");
        if (await link.CountAsync() > 0)
        {
            await link.First.ClickAsync();
        }
        else
        {
            // Fallback: кликаем по ссылке с ведущим /
            var linkWithSlash = page.Locator($"a[href='/{href}']");
            if (await linkWithSlash.CountAsync() > 0)
                await linkWithSlash.First.ClickAsync();
            else
                throw new InvalidOperationException(
                    $"Навигация: ссылка с href='{href}' не найдена в меню. " +
                    $"Добавьте пункт меню для {path}.");
        }
        await AuthHelper.WaitForBlazorReady(page);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle,
            new() { Timeout = NetworkIdleTimeoutMs });
    }

    private static async Task VerifyPageAsync(IPage page, string path, string expectedText, string label)
    {
        await NavigateToPageAsync(page, path);

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
        var now = DateTimeOffset.UtcNow;
        await AuditLogHelper.AssertPageAccessLoggedAsync(path, _verifyStartTime, now);
    }

    private static async Task VerifyButtonAsync(IPage page, string path, string selector, string buttonText, string label)
    {
        if (!page.Url.Contains(path))
        {
            await NavigateToPageAsync(page, path);
        }

        var button = await page.QuerySelectorAsync($"{selector}:has-text('{buttonText}')");
        button.Should().NotBeNull($"{label}: на странице {path} должна быть кнопка «{buttonText}»");
    }

    private static async Task VerifyContentAnyAsync(IPage page, string path, string[] expectedTexts, string label)
    {
        if (!page.Url.Contains(path))
        {
            await NavigateToPageAsync(page, path);
        }

        var content = await page.ContentAsync();
        var found = expectedTexts.Any(t => content.Contains(t));
        found.Should().BeTrue(
            $"{label}: страница {path} должна содержать один из: {string.Join(", ", expectedTexts)}");
    }

    private static async Task VerifyVisibleAsync(IPage page, string path, string selector, string label)
    {
        if (!page.Url.Contains(path))
        {
            await NavigateToPageAsync(page, path);
        }

        var visible = await page.IsVisibleAsync(selector);
        visible.Should().BeTrue($"{label}: на странице {path} должен быть видим элемент «{selector}»");
    }
}
