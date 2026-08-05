using FluentAssertions;
using Microsoft.Playwright;

namespace SamorodinkaTech.Fiducia.Tests.Functional;

/// <summary>
/// US-002: Управление заседаниями — E2E-тесты через Playwright.
/// </summary>
public class US002_MeetingTests : BrowserFixture
{
    [Fact]
    public async Task BoardPortal_MeetingsPage_ShouldLoadAndContainBlazor()
    {
        var page = await CreateBoardPortalPageAsync("/meetings");
        var content = await page.ContentAsync();
        content.Should().Contain("_framework/blazor.server.js");
        content.Should().Contain("Созывы заседаний СД").And.Contain("+ Создать уведомление");
    }

    [Fact]
    public async Task BoardPortal_MeetingsPage_HasCreateButton()
    {
        var page = await CreateBoardPortalPageAsync("/meetings");
        (await page.QuerySelectorAsync("button.btn-primary:has-text('Создать уведомление')"))
            .Should().NotBeNull();
    }

    [Fact]
    public async Task BoardPortal_VotingPage_RenderedOrDefault()
    {
        var page = await CreateBoardPortalPageAsync("/voting/00000000-0000-0000-0000-000000000000");

        var content = await page.ContentAsync();
        content.Should().Contain("_framework/blazor.server.js");
        (content.Contains("spinner-border text-primary") ||
         content.Contains("btn-outline-success") ||
         content.Contains("Голосование"))
             .Should().BeTrue("Страница должна рендерить либо buttons, либо spinner");
    }

    [Fact]
    public async Task AdminConsole_OsaMeetingsPage_ShouldExist()
    {
        var page = await CreateAdminConsolePageAsync("/osa-meetings");
        (await page.ContentAsync()).Should().Contain("_framework/blazor.server.js");
    }

    [Fact]
    public async Task AdminConsole_SettingsPageExists()
    {
        var page = await CreateAdminConsolePageAsync("/settings");
        (await page.ContentAsync()).Should().Contain("_framework/blazor.server.js");
    }

    [Fact]
    public async Task AdminConsole_UserManagementPageRendered()
    {
        var page = await CreateAdminConsolePageAsync("/users");
        (await page.ContentAsync()).Should().Contain("_framework/blazor.server.js");
    }

    [Fact]
    public async Task AdminConsole_AuditPageRendered()
    {
        var page = await CreateAdminConsolePageAsync("/audit");
        (await page.ContentAsync()).Should().Contain("_framework/blazor.server.js");
    }
}
