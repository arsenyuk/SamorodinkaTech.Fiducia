using FluentAssertions;
using Microsoft.Playwright;
using SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

namespace SamorodinkaTech.Fiducia.Tests.Functional;

/// <summary>
/// E2E-тесты интеграции ЕДИН (MPI) — проверка UI-элементов.
/// Требуют запущенных порталов и Playwright.
/// </summary>
public class E2E_EdinIntegrationTests : BrowserFixture
{
    public E2E_EdinIntegrationTests(GlobalFixture globalFixture) : base(globalFixture)
    {
    }

    /// <summary>
    /// Список пользователей содержит столбец «ЕДИН».
    /// </summary>
    [Fact]
    public async Task UsersList_ShouldHaveEdinColumn()
    {
        var page = await CreateAdminConsolePageAsync("/login");
        await AuthHelper.LoginAsAdminAsync(page, "v.vasilyeva", "1");
        await page.GotoAsync(PortalUrls.GetUrl(Portal.AdminConsole, "/users"));
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var header = await page.QuerySelectorAsync("th:text('ЕДИН')");
        header.Should().NotBeNull("столбец ЕДИН должен присутствовать в таблице пользователей");

        await page.CloseAsync();
    }

    /// <summary>
    /// Страница пользователя содержит вкладку «ЕДИН».
    /// </summary>
    [Fact]
    public async Task UserDetail_ShouldHaveEdinTab()
    {
        var page = await CreateAdminConsolePageAsync("/login");
        await AuthHelper.LoginAsAdminAsync(page, "v.vasilyeva", "1");
        await page.GotoAsync(PortalUrls.GetUrl(Portal.AdminConsole, "/users"));
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Клик по первому пользователю в списке
        var firstRow = await page.QuerySelectorAsync("tbody tr");
        if (firstRow is null)
        {
            await page.CloseAsync();
            return;
        }
        await firstRow.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var edinTab = await page.QuerySelectorAsync("button:text('ЕДИН')");
        edinTab.Should().NotBeNull("вкладка ЕДИН должна присутствовать на странице пользователя");

        await page.CloseAsync();
    }

    /// <summary>
    /// Вкладка «ЕДИН» отображает MPI MasterId (или «Не привязан»).
    /// </summary>
    [Fact]
    public async Task EdinTab_ShouldShowMpiMasterIdOrNotLinked()
    {
        var page = await CreateAdminConsolePageAsync("/login");
        await AuthHelper.LoginAsAdminAsync(page, "v.vasilyeva", "1");
        await page.GotoAsync(PortalUrls.GetUrl(Portal.AdminConsole, "/users"));
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var firstRow = await page.QuerySelectorAsync("tbody tr");
        if (firstRow is null)
        {
            await page.CloseAsync();
            return;
        }

        await firstRow.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var edinTab = await page.QuerySelectorAsync("button:text('ЕДИН')");
        if (edinTab is null)
        {
            await page.CloseAsync();
            return;
        }

        await edinTab.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var content = await page.ContentAsync();
        content.Should().Match(
            c => c.Contains("MPI MasterId") || c.Contains("Не привязан"),
            "вкладка ЕДИН должна содержать MPI MasterId или статус «Не привязан»");

        await page.CloseAsync();
    }
}
