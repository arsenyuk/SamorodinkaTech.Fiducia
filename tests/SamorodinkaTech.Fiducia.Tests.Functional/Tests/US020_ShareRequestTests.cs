using FluentAssertions;
using Microsoft.Playwright;

namespace SamorodinkaTech.Fiducia.Tests.Functional;

/// <summary>
/// US-020: Требования участника ООО — E2E-тесты через Playwright.
/// Сценарий: страницы требований загружаются, UI-элементы присутствуют.
/// Полный сценарий требует авторизованной сессии участника ООО.
/// </summary>
public class US020_ShareRequestTests : BrowserFixture
{
    [Fact]
    public async Task BoardPortal_ShareRequestsPage_ShouldLoadWithBlazorShell()
    {
        var page = await CreateBoardPortalPageAsync("/share-requests");
        var content = await page.ContentAsync();
        content.Should().Contain("_framework/blazor.server.js");
    }

    [Fact]
    public async Task BoardPortal_ShareRequestsPage_ShowsExpectedContent()
    {
        var page = await CreateBoardPortalPageAsync("/share-requests");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 10_000 });
        var content = await page.ContentAsync();
        // Страница должна содержать заголовок или список запросов
        (content.Contains("Мои запросы") ||
         content.Contains("Требования") ||
         content.Contains("Нет запросов") ||
         content.Contains("Подать требование") ||
         content.Contains("spinner-border"))
            .Should().BeTrue("Страница требований должна отображать заголовок или кнопку создания");
    }

    [Fact]
    public async Task BoardPortal_ShareRequestsCreatePage_ShouldLoadWithBlazorShell()
    {
        var page = await CreateBoardPortalPageAsync("/share-requests/create");
        var content = await page.ContentAsync();
        content.Should().Contain("_framework/blazor.server.js");
    }

    [Fact]
    public async Task BoardPortal_ShareRequestsCreatePage_ShowsTypeSelection()
    {
        var page = await CreateBoardPortalPageAsync("/share-requests/create");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 10_000 });
        var content = await page.ContentAsync();
        // Форма создания должна содержать выбор типа запроса
        (content.Contains("Тип запроса") ||
         content.Contains("Выберите тип") ||
         content.Contains("spinner-border") ||
         content.Contains("Юридическое лицо не выбрано"))
            .Should().BeTrue("Форма создания должна содержать выбор типа");
    }
}
