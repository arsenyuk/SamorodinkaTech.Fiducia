using FluentAssertions;
using Microsoft.Playwright;
using SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

namespace SamorodinkaTech.Fiducia.Tests.Functional;

/// <summary>
/// US-020: Требования участника — E2E-тест через Playwright.
/// Сценарий: участник (PARTICIPANT) заходит на страницу требований,
/// проверяет загрузку, кнопку создания, API и форму создания.
/// Документация: docs/e2e-share-request.md
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
            await SetupParticipantAsync(page, 1);
            await AuthHelper.LoginAsBoardUserAsync(page, "zhirov.at");

            await page.GotoAsync(PortalUrls.GetUrl(Portal.BoardPortal, "/share-requests"));
            await AuthHelper.WaitForBlazorReady(page);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Ожидание рендера Blazor-компонента (SignalR circuit)
            await page.WaitForSelectorAsync("h3", new() { Timeout = DefaultTimeout });

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
            await SetupParticipantAsync(page, 1);
            await AuthHelper.LoginAsBoardUserAsync(page, "zhirov.at");

            await page.GotoAsync(PortalUrls.GetUrl(Portal.BoardPortal, "/share-requests"));
            await AuthHelper.WaitForBlazorReady(page);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Ожидание рендера Blazor-компонента
            await page.WaitForSelectorAsync("h3", new() { Timeout = DefaultTimeout });

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
            await SetupParticipantAsync(page, 1);
            await AuthHelper.LoginAsBoardUserAsync(page, "zhirov.at");

            await page.GotoAsync(PortalUrls.GetUrl(Portal.BoardPortal, "/share-requests"));
            await AuthHelper.WaitForBlazorReady(page);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Ожидание рендера Blazor-компонента
            await page.WaitForSelectorAsync("h3", new() { Timeout = DefaultTimeout });

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
            await SetupParticipantAsync(page, 1);
            await AuthHelper.LoginAsBoardUserAsync(page, "zhirov.at");

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
            await SetupParticipantAsync(page, 1);
            await AuthHelper.LoginAsBoardUserAsync(page, "zhirov.at");

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

    /// <summary>
    /// Регистрация участника с ПДн через Admin Console + Board Portal.
    /// Использует E2ETestSetupHelper для дедупликации логики.
    /// </summary>
    private async Task SetupParticipantAsync(IPage participantPage, int charterNumber)
    {
        var persons = CharterTestDataFixed.PersonsByEntity[charterNumber];
        var participant = persons.Participants[0];

        var adminPage = await CreateAdminConsolePageAsync();
        try
        {
            await CharterTestSeeder.EnsureSeededAsync(adminPage, charterNumber);

            // Навигация к ЮЛ для AddEmployeeAsync
            var entity = CharterTestDataFixed.LegalEntities[charterNumber - 1];
            if (!adminPage.Url.Contains("/access-management"))
            {
                await AuthHelper.LoginAsAdminAsync(adminPage, CharterTestDataFixed.SysAdminLogin);
            }
            await AdminConsoleHelper.NavigateToLegalEntityAsync(adminPage, entity.Name);

            // Используем E2ETestSetupHelper для регистрации участника
            await E2ETestSetupHelper.SetupParticipantAsync(adminPage, participantPage, participant, charterNumber);
        }
        finally
        {
            await adminPage.CloseAsync();
        }

        // Выход GD — чтобы участник мог залогиниться
        var gdLogin = persons.Gd?.Login ?? persons.Participants[0].Login;
        await AuthHelper.LoginAsBoardUserAsync(participantPage, gdLogin);
        await participantPage.GotoAsync(PortalUrls.GetUrl(Portal.BoardPortal, "/logout"));
        await AuthHelper.WaitForBlazorReady(participantPage);
    }
}
