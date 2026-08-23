using FluentAssertions;
using Microsoft.Playwright;

namespace SamorodinkaTech.Fiducia.Tests.Functional;

/// <summary>
/// US-024: Договоры (включая управляющих ИП для ООО) — E2E-тесты через Playwright.
/// Сценарий: страница договоров загружается, UI-элементы присутствуют.
/// Полный сценарий требует авторизованной сессии.
/// </summary>
public class US024_ContractTests : BrowserFixture
{
    [Fact]
    public async Task BoardPortal_ContractsPage_ShouldLoadWithBlazorShell()
    {
        var page = await CreateBoardPortalPageAsync("/contracts");
        var content = await page.ContentAsync();
        content.Should().Contain("_framework/blazor.server.js");
    }

    [Fact]
    public async Task BoardPortal_ContractsPage_ShowsExpectedContent()
    {
        var page = await CreateBoardPortalPageAsync("/contracts");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 10_000 });
        var content = await page.ContentAsync();
        // Страница договоров должна загрузиться
        (content.Contains("Договоры") ||
         content.Contains("Нет договоров") ||
         content.Contains("spinner-border"))
            .Should().BeTrue("Страница договоров должна отображать заголовок или пустое состояние");
    }
}
