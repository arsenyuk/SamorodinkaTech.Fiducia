using FluentAssertions;
using Microsoft.Playwright;

namespace SamorodinkaTech.Fiducia.Tests.Functional;

/// <summary>
/// US-023: Участники ООО — E2E-тесты через Playwright.
/// Сценарий: страница участников загружается, UI-элементы присутствуют.
/// Полный сценарий требует авторизованной сессии для ООО.
/// </summary>
public class US023_ParticipantTests : BrowserFixture
{
    [Fact]
    public async Task BoardPortal_ParticipantsPage_ShouldLoadWithBlazorShell()
    {
        var page = await CreateBoardPortalPageAsync("/participants");
        var content = await page.ContentAsync();
        content.Should().Contain("_framework/blazor.server.js");
    }

    [Fact]
    public async Task BoardPortal_ParticipantsPage_ShowsExpectedContent()
    {
        var page = await CreateBoardPortalPageAsync("/participants");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 10_000 });
        var content = await page.ContentAsync();
        // Страница участников должна загрузиться без ошибок 5xx
        (content.Contains("Участники") ||
         content.Contains("Нет участников") ||
         content.Contains("spinner-border") ||
         await page.IsVisibleAsync(".container-fluid"))
            .Should().BeTrue("Страница участников должна отображать контент или загрузку");
    }
}
