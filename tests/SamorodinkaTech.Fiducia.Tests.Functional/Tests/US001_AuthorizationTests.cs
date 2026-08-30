using FluentAssertions;
using Microsoft.Playwright;

namespace SamorodinkaTech.Fiducia.Tests.Functional;

/// <summary>
/// US-001: Авторизация — E2E-тесты через Playwright.
/// Проверяет страницы login, публичность AuthLayout.
/// </summary>
public class US001_AuthorizationTests : BrowserFixture
{
    public US001_AuthorizationTests(GlobalFixture globalFixture) : base(globalFixture)
    {
    }

    [Fact]
    public async Task BoardPortal_LoginPage_ShowsSelectDropdown()
        => await AssertLoginDropdownAsync(Portal.BoardPortal);

    [Fact]
    public async Task AdminConsoleLoginPage_LoadsBlazorShell()
    {
        var page = await CreateAdminConsolePageAsync("/login");
        var content = await page.ContentAsync();
        content.Should().Contain("_framework/blazor.server.js", "Login — часть Admin Console SPA");
    }

    [Fact]
    public async Task BoardPortal_LoginPage_ShowsNoSidebar()
    {
        var page = await CreateBoardPortalPageAsync("/login");
        var hasSidebar = await page.EvaluateAsync<bool>("() => document.querySelector('.sidebar') !== null");
        hasSidebar.Should().BeFalse("AuthLayout не содержит сайдбар навигации");
    }

    [Fact]
    public async Task BoardPortal_PublicLanding_Present()
    {
        var page = await CreateBoardPortalPageAsync("/");
        var content = await page.ContentAsync();
        content.Should().Contain("Fiducia");
    }

    [Fact]
    public async Task BoardPortal_OnboardingPage_Rendered()
    {
        var page = await CreateBoardPortalPageAsync("/onboarding");
        var content = await page.ContentAsync();
        content.Should().Contain("_framework/blazor.server.js");
    }

    [Fact]
    public async Task BoardPortal_ProposalPage_RenderedForAnonymousUsers()
    {
        var page = await CreateBoardPortalPageAsync("/proposal");
        var content = await page.ContentAsync();
        content.Should().Contain("_framework/blazor.server.js");
    }

    private async Task AssertLoginDropdownAsync(Portal portal)
    {
        var page = await CreatePageAsync(portal, "/login");
        var select = await page.WaitForSelectorAsync("select.form-select", new() { State = WaitForSelectorState.Visible, Timeout = 10_000 });
        select.Should().NotBeNull();
    }
}
