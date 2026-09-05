using FluentAssertions;
using Microsoft.Playwright;

namespace SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

/// <summary>
/// Хелперы для E2E-тестов ЕДИН-интеграции.
/// </summary>
public static class EdinTestHelper
{
    private const int DefaultTimeout = 15_000;

    /// <summary>
    /// Ожидает привязки MPI MasterId к EcosystemParticipant через fire-and-forget хук.
    /// </summary>
    public static async Task WaitForEdinBindingAsync(IPage page, Guid participantId, int timeoutSeconds = 15)
    {
        var deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);

        while (DateTime.UtcNow < deadline)
        {
            var masterId = await page.EvaluateAsync<string?>(
                $@"async () => {{
                    const response = await fetch('/api/participants/{participantId}', {{
                        credentials: 'same-origin'
                    }});
                    if (!response.ok) return null;
                    const data = await response.json();
                    return data.mpiMasterId || data.MpiMasterId || null;
                }}");

            if (masterId != null)
                return;

            await page.WaitForTimeoutAsync(1000);
        }

        Assert.Fail($"ЕДИН binding не завершился за {timeoutSeconds} сек для участника {participantId}");
    }

    /// <summary>
    /// Получить MPI MasterId участника через API.
    /// </summary>
    public static async Task<string?> GetParticipantMpiMasterIdAsync(IPage page, Guid participantId)
    {
        return await page.EvaluateAsync<string?>(
            $@"async () => {{
                const response = await fetch('/api/participants/{participantId}', {{
                    credentials: 'same-origin'
                }});
                if (!response.ok) return null;
                const data = await response.json();
                return data.mpiMasterId || data.MpiMasterId || null;
            }}");
    }

    /// <summary>
    /// Назначить роль пользователю через UI карточки UserDetail.
    /// Переходит на страницу пользователя, выбирает роль из dropdown, кликает «Добавить».
    /// </summary>
    public static async Task AssignRoleViaUserDetailAsync(
        IPage adminPage, Guid userId, string roleCode)
    {
        var userDetailUrl = PortalUrls.GetUrl(Portal.AdminConsole, $"/users/{userId}");
        await adminPage.GotoAsync(userDetailUrl);
        await adminPage.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await WaitForBlazorReady(adminPage);
        await adminPage.WaitForTimeoutAsync(2000);

        // Кликаем вкладку «Роли»
        var rolesTab = await adminPage.QuerySelectorAsync("button:text('Роли')");
        if (rolesTab != null)
        {
            await rolesTab.ClickAsync();
            await adminPage.WaitForTimeoutAsync(1000);
        }

        // Находим select для ролей и выбираем роль по коду
        // Select содержит option вида "Роль (CODE)" — ищем по тексту
        await adminPage.SelectOptionAsync(
            "select.form-select",
            new SelectOptionValue { Label = roleCode });

        await adminPage.WaitForTimeoutAsync(500);

        // Кликаем «Добавить»
        var addBtn = await adminPage.QuerySelectorAsync("button:text('Добавить')");
        if (addBtn != null)
        {
            await addBtn.ClickAsync();
            await adminPage.WaitForTimeoutAsync(2000);
        }
    }

    /// <summary>
    /// Назначить роль через AccessManagement (Employee + Role).
    /// Использует существующий паттерн AddEmployeeAsync.
    /// </summary>
    public static async Task AssignRoleViaAccessManagementAsync(
        IPage adminPage,
        string lastName, string firstName, string middleName,
        string position, string login, string roleCode)
    {
        await AdminConsoleHelper.AddEmployeeAsync(
            adminPage, lastName, firstName, middleName, position, login, roleCode);
    }

    private static async Task WaitForBlazorReady(IPage page)
    {
        try
        {
            await page.WaitForFunctionAsync(
                @"() => document.querySelector('script[src*=""blazor.server.js""]') !== null",
                null,
                new PageWaitForFunctionOptions { Timeout = DefaultTimeout });
        }
        catch
        {
            // Blazor may already be loaded
        }
    }
}
