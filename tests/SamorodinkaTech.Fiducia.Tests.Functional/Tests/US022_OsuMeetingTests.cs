using FluentAssertions;
using Microsoft.Playwright;

namespace SamorodinkaTech.Fiducia.Tests.Functional;

/// <summary>
/// US-022: Общее собрание участников (ОСУ) — E2E-тесты через Playwright.
/// Сценарий: страницы ОСУ загружаются, UI-элементы присутствуют.
/// Полный сценарий требует авторизованной сессии для ООО.
/// </summary>
public class US022_OsuMeetingTests : BrowserFixture
{
    [Fact]
    public async Task BoardPortal_OsuMeetingsPage_ShouldLoadWithBlazorShell()
    {
        var page = await CreateBoardPortalPageAsync("/osu-meetings");
        var content = await page.ContentAsync();
        content.Should().Contain("_framework/blazor.server.js");
    }

    [Fact]
    public async Task BoardPortal_OsuMeetingsPage_ShowsExpectedContent()
    {
        var page = await CreateBoardPortalPageAsync("/osu-meetings");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 10_000 });
        var content = await page.ContentAsync();
        // Страница должна содержать заголовок или список собраний
        (content.Contains("Общие собрания") ||
         content.Contains("ОСУ") ||
         content.Contains("Нет собраний") ||
         content.Contains("Создать") ||
         content.Contains("spinner-border"))
            .Should().BeTrue("Страница ОСУ должна отображать заголовок или кнопку создания");
    }

    [Fact]
    public async Task BoardPortal_AgendaOsuPage_ShouldLoadWithBlazorShell()
    {
        var page = await CreateBoardPortalPageAsync("/agenda-osu");
        var content = await page.ContentAsync();
        content.Should().Contain("_framework/blazor.server.js");
    }

    [Fact]
    public async Task BoardPortal_AgendaOsuPage_ShowsExpectedContent()
    {
        var page = await CreateBoardPortalPageAsync("/agenda-osu");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 10_000 });
        var content = await page.ContentAsync();
        // Страница повестки ОСУ должна загрузиться
        (content.Contains("Повестка") ||
         content.Contains("ОСУ") ||
         content.Contains("Нет вопросов") ||
         content.Contains("spinner-border"))
            .Should().BeTrue("Страница повестки ОСУ должна отображать заголовок или пустое состояние");
    }
}
