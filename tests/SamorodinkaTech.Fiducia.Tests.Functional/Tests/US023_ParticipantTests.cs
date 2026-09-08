using FluentAssertions;
using Microsoft.Playwright;
using SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

namespace SamorodinkaTech.Fiducia.Tests.Functional;

/// <summary>
/// US-023: Участники ООО — E2E-тест через Playwright.
/// Сценарий: участник (PARTICIPANT) заходит на страницу участников общества,
/// проверяет загрузку, вкладки, API и отсутствие ошибок.
/// Страница доступна только для ООО (ОКОПФ 12300).
/// </summary>
public class US023_ParticipantTests : BrowserFixture
{
    public US023_ParticipantTests(GlobalFixture globalFixture) : base(globalFixture)
    {
    }

    /// <summary>
    /// US-023: Страница участников загружается для участника ООО.
    /// </summary>
    [Fact]
    public async Task BoardPortal_Participants_ShouldLoadWithExpectedContent()
    {
        SkipIfPreviousFailed();

        var page = await CreateBoardPortalPageAsync();
        try
        {
            var adminPage = await CreateAdminConsolePageAsync();
            try { await CharterTestSeeder.EnsureSeededAsync(adminPage, 1); }
            finally { await adminPage.CloseAsync(); }

            await AuthHelper.LoginAsBoardUserAsync(page, "zhirov.at1");

            await page.GotoAsync(PortalUrls.GetUrl(Portal.BoardPortal, "/participants"));
            await AuthHelper.WaitForBlazorReady(page);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var content = await page.ContentAsync();

            content.Should().Contain("_framework/blazor.server.js",
                "Board Portal: Participants должен содержать Blazor shell");

            content.Should().Contain("Участники",
                "Board Portal: Participants должен содержать заголовок");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    /// <summary>
    /// US-023: Страница участников содержит вкладки для ООО.
    /// </summary>
    [Fact]
    public async Task BoardPortal_Participants_ShouldHaveTabsForLLC()
    {
        SkipIfPreviousFailed();

        var page = await CreateBoardPortalPageAsync();
        try
        {
            var adminPage = await CreateAdminConsolePageAsync();
            try { await CharterTestSeeder.EnsureSeededAsync(adminPage, 1); }
            finally { await adminPage.CloseAsync(); }

            await AuthHelper.LoginAsBoardUserAsync(page, "zhirov.at1");

            await page.GotoAsync(PortalUrls.GetUrl(Portal.BoardPortal, "/participants"));
            await AuthHelper.WaitForBlazorReady(page);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var content = await page.ContentAsync();

            content.Should().Contain("Участники общества",
                "Board Portal: Participants должен содержать вкладку «Участники общества»");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    /// <summary>
    /// US-023: Страница участников не показывает NotFound.
    /// </summary>
    [Fact]
    public async Task BoardPortal_Participants_ShouldNotShowNotFound()
    {
        SkipIfPreviousFailed();

        var page = await CreateBoardPortalPageAsync();
        try
        {
            var adminPage = await CreateAdminConsolePageAsync();
            try { await CharterTestSeeder.EnsureSeededAsync(adminPage, 1); }
            finally { await adminPage.CloseAsync(); }

            await AuthHelper.LoginAsBoardUserAsync(page, "zhirov.at1");

            await page.GotoAsync(PortalUrls.GetUrl(Portal.BoardPortal, "/participants"));
            await AuthHelper.WaitForBlazorReady(page);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var content = await page.ContentAsync();

            content.Should().NotContain("Sorry, there's nothing at this address.",
                "Board Portal: Participants не должен показывать NotFound");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    /// <summary>
    /// US-023: API /api/participants отвечает 200 и возвращает массив.
    /// </summary>
    [Fact]
    public async Task BoardPortal_Participants_ApiShouldReturn200()
    {
        SkipIfPreviousFailed();

        var page = await CreateBoardPortalPageAsync();
        try
        {
            var adminPage = await CreateAdminConsolePageAsync();
            try { await CharterTestSeeder.EnsureSeededAsync(adminPage, 1); }
            finally { await adminPage.CloseAsync(); }

            await AuthHelper.LoginAsBoardUserAsync(page, "zhirov.at1");

            await page.GotoAsync(PortalUrls.GetUrl(Portal.BoardPortal, "/participants"));
            await AuthHelper.WaitForBlazorReady(page);

            var response = await page.EvaluateAsync<int>(
                @"async () => {
                    const resp = await fetch('/api/participants', {
                        method: 'GET',
                        credentials: 'same-origin'
                    });
                    return resp.status;
                }");

            response.Should().Be(200,
                "Board Portal: API /api/participants должен отвечать 200");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    /// <summary>
    /// US-023: API /api/participants возвращает JSON-массив.
    /// </summary>
    [Fact]
    public async Task BoardPortal_Participants_ApiShouldReturnArray()
    {
        SkipIfPreviousFailed();

        var page = await CreateBoardPortalPageAsync();
        try
        {
            var adminPage = await CreateAdminConsolePageAsync();
            try { await CharterTestSeeder.EnsureSeededAsync(adminPage, 1); }
            finally { await adminPage.CloseAsync(); }

            await AuthHelper.LoginAsBoardUserAsync(page, "zhirov.at1");

            await page.GotoAsync(PortalUrls.GetUrl(Portal.BoardPortal, "/participants"));
            await AuthHelper.WaitForBlazorReady(page);

            var result = await page.EvaluateAsync<bool>(
                @"async () => {
                    const resp = await fetch('/api/participants', {
                        method: 'GET',
                        credentials: 'same-origin'
                    });
                    if (!resp.ok) return false;
                    const data = await resp.json();
                    return Array.isArray(data);
                }");

            result.Should().BeTrue(
                "Board Portal: API /api/participants должен возвращать JSON-массив");
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
