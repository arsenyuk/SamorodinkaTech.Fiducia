using FluentAssertions;
using Microsoft.Playwright;

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
        var page = await CreateAdminConsolePageAsync("/users");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var header = await page.QuerySelectorAsync("th:text('ЕДИН')");
        header.Should().NotBeNull("столбец ЕДИН должен присутствовать в таблице пользователей");
    }

    /// <summary>
    /// Страница пользователя содержит вкладку «ЕДИН».
    /// </summary>
    [Fact]
    public async Task UserDetail_ShouldHaveEdinTab()
    {
        var page = await CreateAdminConsolePageAsync("/users");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Клик по первому пользователю в списке
        var firstRow = await page.QuerySelectorAsync("tbody tr");
        if (firstRow is null)
        {
            // Нет пользователей — пропускаем
            return;
        }
        await firstRow.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var edinTab = await page.QuerySelectorAsync("button:text('ЕДИН')");
        edinTab.Should().NotBeNull("вкладка ЕДИН должна присутствовать на странице пользователя");
    }

    /// <summary>
    /// Вкладка «ЕДИН» отображает MPI MasterId (или «Не привязан»).
    /// </summary>
    [Fact]
    public async Task EdinTab_ShouldShowMpiMasterIdOrNotLinked()
    {
        var page = await CreateAdminConsolePageAsync("/users");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var firstRow = await page.QuerySelectorAsync("tbody tr");
        if (firstRow is null) return;

        await firstRow.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var edinTab = await page.QuerySelectorAsync("button:text('ЕДИН')");
        if (edinTab is null) return;

        await edinTab.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var content = await page.ContentAsync();
        content.Should().Match(
            c => c.Contains("MPI MasterId") || c.Contains("Не привязан"),
            "вкладка ЕДИН должна содержать MPI MasterId или статус «Не привязан»");
    }
}
