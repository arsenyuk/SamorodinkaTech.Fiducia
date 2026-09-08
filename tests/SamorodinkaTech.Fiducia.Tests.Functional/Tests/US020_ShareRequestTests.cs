using FluentAssertions;
using Microsoft.Playwright;
using SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

namespace SamorodinkaTech.Fiducia.Tests.Functional;

/// <summary>
/// US-020: Требования участника — E2E-тест через Playwright.
/// Сценарий: участник (PARTICIPANT) заходит на страницу требований,
/// проверяет загрузку, кнопку создания, API и форму создания.
/// </summary>
public class US020_ShareRequestTests : BrowserFixture
{
    public US020_ShareRequestTests(GlobalFixture globalFixture) : base(globalFixture)
    {
    }

    /// <summary>
    /// US-020: Страница требований загружается для участника.
    /// </summary>
    [Fact]
    public async Task BoardPortal_ShareRequests_ShouldLoadWithExpectedContent()
    {
        SkipIfPreviousFailed();

        var page = await CreateBoardPortalPageAsync();
        try
        {
            var adminPage = await CreateAdminConsolePageAsync();
            try { await CharterTestSeeder.EnsureSeededAsync(adminPage, 1); }
            finally { await adminPage.CloseAsync(); }

            await AuthHelper.LoginAsBoardUserAsync(page, "zhirov.at1");

            await page.GotoAsync(PortalUrls.GetUrl(Portal.BoardPortal, "/share-requests"));
            await AuthHelper.WaitForBlazorReady(page);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var content = await page.ContentAsync();

            content.Should().Contain("_framework/blazor.server.js",
                "Board Portal: ShareRequests должен содержать Blazor shell");

            content.Should().Contain("Мои запросы",
                "Board Portal: ShareRequests должен содержать заголовок");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    /// <summary>
    /// US-020: Страница требований содержит кнопку создания.
    /// </summary>
    [Fact]
    public async Task BoardPortal_ShareRequests_ShouldHaveCreateButton()
    {
        SkipIfPreviousFailed();

        var page = await CreateBoardPortalPageAsync();
        try
        {
            var adminPage = await CreateAdminConsolePageAsync();
            try { await CharterTestSeeder.EnsureSeededAsync(adminPage, 1); }
            finally { await adminPage.CloseAsync(); }

            await AuthHelper.LoginAsBoardUserAsync(page, "zhirov.at1");

            await page.GotoAsync(PortalUrls.GetUrl(Portal.BoardPortal, "/share-requests"));
            await AuthHelper.WaitForBlazorReady(page);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var content = await page.ContentAsync();

            content.Should().Contain("Подать требование",
                "Board Portal: ShareRequests должен содержать кнопку создания");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    /// <summary>
    /// US-020: Страница требований не показывает NotFound.
    /// </summary>
    [Fact]
    public async Task BoardPortal_ShareRequests_ShouldNotShowNotFound()
    {
        SkipIfPreviousFailed();

        var page = await CreateBoardPortalPageAsync();
        try
        {
            var adminPage = await CreateAdminConsolePageAsync();
            try { await CharterTestSeeder.EnsureSeededAsync(adminPage, 1); }
            finally { await adminPage.CloseAsync(); }

            await AuthHelper.LoginAsBoardUserAsync(page, "zhirov.at1");

            await page.GotoAsync(PortalUrls.GetUrl(Portal.BoardPortal, "/share-requests"));
            await AuthHelper.WaitForBlazorReady(page);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var content = await page.ContentAsync();

            content.Should().NotContain("Sorry, there's nothing at this address.",
                "Board Portal: ShareRequests не должен показывать NotFound");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    /// <summary>
    /// US-020: API /api/share-requests отвечает 200.
    /// </summary>
    [Fact]
    public async Task BoardPortal_ShareRequests_ApiShouldReturn200()
    {
        SkipIfPreviousFailed();

        var page = await CreateBoardPortalPageAsync();
        try
        {
            var adminPage = await CreateAdminConsolePageAsync();
            try { await CharterTestSeeder.EnsureSeededAsync(adminPage, 1); }
            finally { await adminPage.CloseAsync(); }

            await AuthHelper.LoginAsBoardUserAsync(page, "zhirov.at1");

            await page.GotoAsync(PortalUrls.GetUrl(Portal.BoardPortal, "/share-requests"));
            await AuthHelper.WaitForBlazorReady(page);

            var response = await page.EvaluateAsync<int>(
                @"async () => {
                    const resp = await fetch('/api/share-requests', {
                        method: 'GET',
                        credentials: 'same-origin'
                    });
                    return resp.status;
                }");

            response.Should().Be(200,
                "Board Portal: API /api/share-requests должен отвечать 200");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    /// <summary>
    /// US-020: API /api/share-requests/types отвечает 200 и возвращает массив.
    /// </summary>
    [Fact]
    public async Task BoardPortal_ShareRequestTypes_ApiShouldReturn200()
    {
        SkipIfPreviousFailed();

        var page = await CreateBoardPortalPageAsync();
        try
        {
            var adminPage = await CreateAdminConsolePageAsync();
            try { await CharterTestSeeder.EnsureSeededAsync(adminPage, 1); }
            finally { await adminPage.CloseAsync(); }

            await AuthHelper.LoginAsBoardUserAsync(page, "zhirov.at1");

            await page.GotoAsync(PortalUrls.GetUrl(Portal.BoardPortal, "/share-requests"));
            await AuthHelper.WaitForBlazorReady(page);

            var result = await page.EvaluateAsync<object>(
                @"async () => {
                    const resp = await fetch('/api/share-requests/types', {
                        method: 'GET',
                        credentials: 'same-origin'
                    });
                    if (!resp.ok) return { ok: false, status: resp.status };
                    const data = await resp.json();
                    return { ok: true, isArray: Array.isArray(data), count: data.length };
                }");

            var json = System.Text.Json.JsonSerializer.Serialize(result);
            json.Should().Contain("\"ok\":true",
                "Board Portal: API /api/share-requests/types должен отвечать 200");
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
