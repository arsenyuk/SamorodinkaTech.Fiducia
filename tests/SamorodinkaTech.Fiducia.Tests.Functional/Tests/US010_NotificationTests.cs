using FluentAssertions;
using Microsoft.Playwright;

namespace SamorodinkaTech.Fiducia.Tests.Functional;

/// <summary>
/// US-010: Оповещения — E2E-тесты через Playwright.
/// Сценарий: страница оповещений загружается, UI-элементы присутствуют.
/// Полный сценарий требует авторизованной сессии с ролью PARTICIPANT.
/// </summary>
public class US010_NotificationTests : BrowserFixture
{
    [Fact]
    public async Task BoardPortal_NotificationsPage_ShouldLoadWithBlazorShell()
    {
        var page = await CreateBoardPortalPageAsync("/notifications");
        var content = await page.ContentAsync();
        content.Should().Contain("_framework/blazor.server.js");
    }

    [Fact]
    public async Task BoardPortal_NotificationsPage_ShowsExpectedContent()
    {
        var page = await CreateBoardPortalPageAsync("/notifications");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 10_000 });
        var content = await page.ContentAsync();
        // Страница должна содержать заголовок или список уведомлений
        (content.Contains("Оповещения") ||
         content.Contains("Уведомления") ||
         content.Contains("Нет уведомлений") ||
         content.Contains("spinner-border"))
            .Should().BeTrue("Страница оповещений должна отображать заголовок или состояние");
    }
}
