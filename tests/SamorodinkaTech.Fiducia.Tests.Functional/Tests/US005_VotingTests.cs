using FluentAssertions;
using Microsoft.Playwright;

namespace SamorodinkaTech.Fiducia.Tests.Functional;

/// <summary>
/// US-005: Голосование — E2E-тесты через Playwright.
/// Сценарий: страница голосования загружается, UI-элементы присутствуют.
/// Полный сценарий требует авторизованной сессии и активного голосования.
/// </summary>
public class US005_VotingTests : BrowserFixture
{
    [Fact]
    public async Task BoardPortal_VotingPage_ShouldLoadWithBlazorShell()
    {
        var page = await CreateBoardPortalPageAsync("/voting/00000000-0000-0000-0000-000000000000");
        var content = await page.ContentAsync();
        content.Should().Contain("_framework/blazor.server.js");
    }

    [Fact]
    public async Task BoardPortal_VotingPage_RendersSpinnerOrContent()
    {
        var page = await CreateBoardPortalPageAsync("/voting/00000000-0000-0000-0000-000000000000");
        var content = await page.ContentAsync();
        // Страница должна рендерить либо spinner (загрузка), либо кнопки голосования, либо сообщение
        (content.Contains("spinner-border") ||
         content.Contains("btn-outline-success") ||
         content.Contains("Голосование") ||
         content.Contains("Не найдено"))
            .Should().BeTrue("Страница голосования должна отображать хотя бы один из состояний");
    }
}
