using FluentAssertions;
using Microsoft.Playwright;

namespace SamorodinkaTech.Fiducia.Tests.Functional;

/// <summary>
/// US-021: Каталог предоставленных документов — E2E-тесты через Playwright.
/// Сценарий: страница каталога загружается, UI-элементы присутствуют.
/// Полный сценарий требует авторизованной сессии участника с принятыми требованиями.
/// </summary>
public class US021_DocumentCatalogTests : BrowserFixture
{
    [Fact]
    public async Task BoardPortal_DocumentsCatalogPage_ShouldLoadWithBlazorShell()
    {
        var page = await CreateBoardPortalPageAsync("/documents/catalog");
        var content = await page.ContentAsync();
        content.Should().Contain("_framework/blazor.server.js");
    }

    [Fact]
    public async Task BoardPortal_DocumentsCatalogPage_ShowsExpectedContent()
    {
        var page = await CreateBoardPortalPageAsync("/documents/catalog");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 10_000 });
        var content = await page.ContentAsync();
        // Страница должна содержать заголовок или список документов
        (content.Contains("Предоставленные документы") ||
         content.Contains("Нет предоставленных") ||
         content.Contains("accordion") ||
         content.Contains("spinner-border") ||
         content.Contains("Юридическое лицо не выбрано"))
            .Should().BeTrue("Страница каталога должна отображать заголовок или пустое состояние");
    }

    [Fact]
    public async Task BoardPortal_DocumentsCatalogPage_HasAccordionStructure()
    {
        var page = await CreateBoardPortalPageAsync("/documents/catalog");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 10_000 });
        var content = await page.ContentAsync();
        // Если есть документы — должна быть accordion-структура
        // Если нет — должно быть сообщение о пустом списке
        (content.Contains("accordion") ||
         content.Contains("Нет предоставленных") ||
         content.Contains("Юридическое лицо не выбрано"))
            .Should().BeTrue("Страница должна содержать accordion или сообщение о пустом списке");
    }
}
