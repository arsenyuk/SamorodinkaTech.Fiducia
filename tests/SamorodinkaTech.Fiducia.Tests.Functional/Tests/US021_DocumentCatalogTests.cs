using FluentAssertions;
using Microsoft.Playwright;
using SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

namespace SamorodinkaTech.Fiducia.Tests.Functional;

/// <summary>
/// US-021: Каталог предоставленных документов — E2E-тест через Playwright.
/// Сценарий: участник (PARTICIPANT) заходит на страницу каталога документов,
/// проверяет загрузку страницы, наличие accordion-элементов и ссылок на скачивание.
/// Требуется роль PARTICIPANT в Board Portal.
/// </summary>
public class US021_DocumentCatalogTests : BrowserFixture
{
    public US021_DocumentCatalogTests(GlobalFixture globalFixture) : base(globalFixture)
    {
    }

    /// <summary>
    /// US-021: Страница каталога документов загружается для участника.
    /// Проверяет: Blazor shell загружается, заголовок присутствует.
    /// </summary>
    [Fact]
    public async Task BoardPortal_DocumentCatalog_ShouldLoadWithExpectedContent()
    {
        SkipIfPreviousFailed();

        var page = await CreateBoardPortalPageAsync();
        try
        {
            // Сидировать ЮЛ 1 (создаёт пользователей через Admin Console)
            var adminPage = await CreateAdminConsolePageAsync();
            try
            {
                await CharterTestSeeder.EnsureSeededAsync(adminPage, 1);
            }
            finally
            {
                await adminPage.CloseAsync();
            }

            // Логин как PARTICIPANT (zhirov.at1 — участник ЮЛ 1)
            await AuthHelper.LoginAsBoardUserAsync(page, "zhirov.at1");

            await page.GotoAsync(PortalUrls.GetUrl(Portal.BoardPortal, "/documents/catalog"));
            await AuthHelper.WaitForBlazorReady(page);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var content = await page.ContentAsync();

            // Blazor shell загрузился
            content.Should().Contain("_framework/blazor.server.js",
                "Board Portal: DocumentsCatalog должен содержать Blazor shell");

            // Заголовок страницы
            content.Should().Contain("Предоставленные документы",
                "Board Portal: DocumentsCatalog должен содержать заголовок");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    /// <summary>
    /// US-021: Страница каталога документов содержит accordion-структуру или сообщение об отсутствии документов.
    /// </summary>
    [Fact]
    public async Task BoardPortal_DocumentCatalog_ShouldHaveAccordionOrEmptyState()
    {
        SkipIfPreviousFailed();

        var page = await CreateBoardPortalPageAsync();
        try
        {
            // Сидировать ЮЛ 1
            var adminPage = await CreateAdminConsolePageAsync();
            try
            {
                await CharterTestSeeder.EnsureSeededAsync(adminPage, 1);
            }
            finally
            {
                await adminPage.CloseAsync();
            }

            await AuthHelper.LoginAsBoardUserAsync(page, "zhirov.at1");

            await page.GotoAsync(PortalUrls.GetUrl(Portal.BoardPortal, "/documents/catalog"));
            await AuthHelper.WaitForBlazorReady(page);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var content = await page.ContentAsync();

            // Проверяем: либо есть accordion, либо сообщение "Нет предоставленных документов"
            var hasAccordion = content.Contains("accordion") || content.Contains("documentCatalog");
            var hasEmptyState = content.Contains("Нет предоставленных документов");

            (hasAccordion || hasEmptyState).Should().BeTrue(
                "Board Portal: DocumentsCatalog должен содержать accordion или сообщение об отсутствии документов");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    /// <summary>
    /// US-021: Страница каталога документов не показывает Blazor Router NotFound.
    /// </summary>
    [Fact]
    public async Task BoardPortal_DocumentCatalog_ShouldNotShowNotFound()
    {
        SkipIfPreviousFailed();

        var page = await CreateBoardPortalPageAsync();
        try
        {
            // Сидировать ЮЛ 1
            var adminPage = await CreateAdminConsolePageAsync();
            try
            {
                await CharterTestSeeder.EnsureSeededAsync(adminPage, 1);
            }
            finally
            {
                await adminPage.CloseAsync();
            }

            await AuthHelper.LoginAsBoardUserAsync(page, "zhirov.at1");

            await page.GotoAsync(PortalUrls.GetUrl(Portal.BoardPortal, "/documents/catalog"));
            await AuthHelper.WaitForBlazorReady(page);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var content = await page.ContentAsync();

            content.Should().NotContain("Sorry, there's nothing at this address.",
                "Board Portal: DocumentsCatalog не должен показывать Blazor Router NotFound");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    /// <summary>
    /// US-021: Проверка API /api/documents/catalog отвечает 200.
    /// </summary>
    [Fact]
    public async Task BoardPortal_DocumentCatalog_ApiShouldReturn200()
    {
        SkipIfPreviousFailed();

        var page = await CreateBoardPortalPageAsync();
        try
        {
            // Сидировать ЮЛ 1
            var adminPage = await CreateAdminConsolePageAsync();
            try
            {
                await CharterTestSeeder.EnsureSeededAsync(adminPage, 1);
            }
            finally
            {
                await adminPage.CloseAsync();
            }

            await AuthHelper.LoginAsBoardUserAsync(page, "zhirov.at1");

            await page.GotoAsync(PortalUrls.GetUrl(Portal.BoardPortal, "/documents/catalog"));
            await AuthHelper.WaitForBlazorReady(page);

            var response = await page.EvaluateAsync<int>(
                @"async () => {
                    const resp = await fetch('/api/documents/catalog', {
                        method: 'GET',
                        credentials: 'same-origin'
                    });
                    return resp.status;
                }");

            response.Should().Be(200,
                "Board Portal: API /api/documents/catalog должен отвечать 200");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    /// <summary>
    /// US-021: API /api/documents/catalog возвращает JSON с полем groups.
    /// </summary>
    [Fact]
    public async Task BoardPortal_DocumentCatalog_ApiShouldReturnGroups()
    {
        SkipIfPreviousFailed();

        var page = await CreateBoardPortalPageAsync();
        try
        {
            // Сидировать ЮЛ 1
            var adminPage = await CreateAdminConsolePageAsync();
            try
            {
                await CharterTestSeeder.EnsureSeededAsync(adminPage, 1);
            }
            finally
            {
                await adminPage.CloseAsync();
            }

            await AuthHelper.LoginAsBoardUserAsync(page, "zhirov.at1");

            await page.GotoAsync(PortalUrls.GetUrl(Portal.BoardPortal, "/documents/catalog"));
            await AuthHelper.WaitForBlazorReady(page);

            var hasGroups = await page.EvaluateAsync<bool>(
                @"async () => {
                    const resp = await fetch('/api/documents/catalog', {
                        method: 'GET',
                        credentials: 'same-origin'
                    });
                    if (!resp.ok) return false;
                    const data = await resp.json();
                    return data && Array.isArray(data.groups);
                }");

            hasGroups.Should().BeTrue(
                "Board Portal: API /api/documents/catalog должен возвращать JSON с полем groups");
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
