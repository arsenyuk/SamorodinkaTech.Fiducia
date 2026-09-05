using FluentAssertions;
using Microsoft.Playwright;
using SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

namespace SamorodinkaTech.Fiducia.Tests.Functional;

/// <summary>
/// E2E-тесты интеграции ЕДИН (MPI) — проверка UI-элементов.
/// Правила: ADR-027 (навигация через UI, проверка контента, аудит, логи).
/// </summary>
public class E2E_EdinIntegrationTests : BrowserFixture
{
    public E2E_EdinIntegrationTests(GlobalFixture globalFixture) : base(globalFixture)
    {
    }

    /// <summary>
    /// US-EDIN-1: Список пользователей содержит столбец «ЕДИН».
    /// </summary>
    [Fact]
    public async Task UsersList_ShouldHaveEdinColumn()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var page = await CreateAdminConsolePageAsync("/login");

        try
        {
            await AuthHelper.LoginAsAdminAsync(page, "v.vasilyeva", "1");

            // Навигация через sidebar (ADR-027: правило 4, AGENTS: GotoAsync запрещён)
            await AdminConsoleHelper.NavigateToAsync(page, "/users");

            // Ожидание рендеринга таблицы
            await page.WaitForSelectorAsync("th", new PageWaitForSelectorOptions { Timeout = 10_000 });

            // Проверка UI-элемента (ADR-027: правило 1)
            var header = await page.QuerySelectorAsync("th:has-text('ЕДИН')");
            if (header is null)
            {
                var url = page.Url;
                var bodyText = await page.EvaluateAsync<string>("() => document.body?.innerText?.substring(0, 300) ?? 'empty'");
                Assert.Fail($"Столбец ЕДИН не найден. URL: {url}. Body: {bodyText}");
            }
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, nameof(UsersList_ShouldHaveEdinColumn));
            await page.CloseAsync();
        }
    }

    /// <summary>
    /// US-EDIN-2: Страница пользователя содержит вкладку «ЕДИН».
    /// </summary>
    [Fact]
    public async Task UserDetail_ShouldHaveEdinTab()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var page = await CreateAdminConsolePageAsync("/login");

        try
        {
            await AuthHelper.LoginAsAdminAsync(page, "v.vasilyeva", "1");

            // Навигация через sidebar
            await AdminConsoleHelper.NavigateToAsync(page, "/users");

            // Ожидание загрузки таблицы
            await page.WaitForSelectorAsync("tbody tr", new PageWaitForSelectorOptions { Timeout = 10_000 });

            // Клик по первому пользователю (Blazor @onclick на <tr>)
            await page.ClickAsync("tbody tr");

            // Ожидание загрузки карточки пользователя
            await page.WaitForSelectorAsync("button:text('УЗ')", new PageWaitForSelectorOptions { Timeout = 10_000 });

            // Проверка наличия вкладки «ЕДИН»
            var edinTab = await page.QuerySelectorAsync("button:text('ЕДИН')");
            edinTab.Should().NotBeNull("вкладка ЕДИН должна присутствовать на странице пользователя");
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, nameof(UserDetail_ShouldHaveEdinTab));
            await page.CloseAsync();
        }
    }

    /// <summary>
    /// US-EDIN-3: Вкладка «ЕДИН» отображает MPI MasterId (или «Не привязан»).
    /// </summary>
    [Fact]
    public async Task EdinTab_ShouldShowMpiMasterIdOrNotLinked()
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var page = await CreateAdminConsolePageAsync("/login");

        try
        {
            await AuthHelper.LoginAsAdminAsync(page, "v.vasilyeva", "1");
            await AdminConsoleHelper.NavigateToAsync(page, "/users");

            await page.WaitForSelectorAsync("tbody tr", new PageWaitForSelectorOptions { Timeout = 10_000 });
            await page.ClickAsync("tbody tr");
            await page.WaitForSelectorAsync("button:text('УЗ')", new PageWaitForSelectorOptions { Timeout = 10_000 });

            var edinTab = await page.QuerySelectorAsync("button:text('ЕДИН')");
            edinTab.Should().NotBeNull("вкладка ЕДИН должна присутствовать");

            await edinTab!.ClickAsync();

            // Ожидание загрузки контента вкладки
            await page.WaitForFunctionAsync(
                "() => document.body.innerText.includes('MPI MasterId') || document.body.innerText.includes('Не привязан')",
                null,
                new PageWaitForFunctionOptions { Timeout = 10_000 });

            var content = await page.ContentAsync();
            content.Should().Match(
                c => c.Contains("MPI MasterId") || c.Contains("Не привязан"),
                "вкладка ЕДИН должна содержать MPI MasterId или статус «Не привязан»");
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, nameof(EdinTab_ShouldShowMpiMasterIdOrNotLinked));
            await page.CloseAsync();
        }
    }
}
