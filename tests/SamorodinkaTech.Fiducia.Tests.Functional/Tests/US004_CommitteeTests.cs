using FluentAssertions;
using Microsoft.Playwright;

namespace SamorodinkaTech.Fiducia.Tests.Functional;

/// <summary>
/// US-004: Управление комитетами и системные настройки.
/// </summary>
public class US004_CommitteeTests : BrowserFixture
{
    [Fact]
    public async Task BoardPortal_CommitteesPage_ShouldLoadAndContainBlazor()
    {
        var page = await CreateBoardPortalPageAsync("/committees");
        var content = await page.ContentAsync();
        content.Should().Contain("_framework/blazor.server.js");
        content.Should().Contain("Комитеты совета директоров").And.Contain("+ Создать комитет");
    }

    [Fact]
    public async Task BoardPortal_CommitteesPage_HasBehaviorTypeDropdown()
    {
        var page = await CreateBoardPortalPageAsync("/committees");
        (await page.ContentAsync())
            .Should().Contain("Защитный").And.Contain("Стратегический");
    }

    [Fact]
    public async Task BoardPortal_DocumentsPage_Rendered()
    {
        var page = await CreateBoardPortalPageAsync("/documents");
        (await page.ContentAsync()).Should().Contain("_framework/blazor.server.js");
    }

    [Fact]
    public async Task BoardPortal_PrintFormsPage_Rendered()
    {
        var page = await CreateBoardPortalPageAsync("/print-forms");
        (await page.ContentAsync()).Should().Contain("_framework/blazor.server.js");
    }

    [Fact]
    public async Task BoardPortal_ParticipantsPage_Loads()
    {
        var page = await CreateBoardPortalPageAsync("/participants");
        // Страница должна загрузиться без ошибок 5xx
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new() { Timeout = 10_000 });
        (await page.IsVisibleAsync(".container-fluid")).Should().BeTrue();
    }

    [Fact]
    public async Task AdminConsole_UsersList_RenderedViaAdminConsole()
    {
        var page = await CreateAdminConsolePageAsync("/users");
        (await page.ContentAsync()).Should().Contain("_framework/blazor.server.js");
    }

    [Fact]
    public async Task AdminConsole_RolesPage_Rendered()
    {
        var page = await CreateAdminConsolePageAsync("/roles");
        (await page.ContentAsync()).Should().Contain("_framework/blazor.server.js");
    }

    [Fact]
    public async Task AdminConsole_OrgTemplatesPageExists()
    {
        var page = await CreateAdminConsolePageAsync("/org-templates");
        (await page.ContentAsync()).Should().Contain("_framework/blazor.server.js");
    }

    [Fact]
    public async Task AdminConsole_SentNotificationsPageExists()
    {
        var page = await CreateAdminConsolePageAsync("/sent-notifications");
        (await page.ContentAsync()).Should().Contain("_framework/blazor.server.js");
    }

    [Fact]
    public async Task AdminConsole_DictionariesPageExists()
    {
        var page = await CreateAdminConsolePageAsync("/dictionaries");
        (await page.ContentAsync()).Should().Contain("_framework/blazor.server.js");
    }
}
