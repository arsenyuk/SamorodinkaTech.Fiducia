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
            await AuthHelper.LoginAsBoardUserAsync(page, "zhirov.at1");

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
            await AuthHelper.LoginAsBoardUserAsync(page, "zhirov.at1");

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
            await AuthHelper.LoginAsBoardUserAsync(page, "zhirov.at1");

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
            await SetupParticipantAsync(page, 1);
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

    /// <summary>
    /// Подготовка тестовых данных: сидирование ЮЛ + регистрация участника через Board Portal.
    /// 1. Admin Console: ЮЛ + LE_ADMIN (CharterTestSeeder)
    /// 2. Admin Console: User для участника (AddEmployeeAsync) — создаёт запись в `users`
    /// 3. Board Portal: GD регистрирует участника с ПДн → ЕДИН binding → PARTICIPANT
    /// </summary>
    private async Task SetupParticipantAsync(IPage participantPage, int charterNumber)
    {
        var entity = CharterTestDataFixed.LegalEntities[charterNumber - 1];
        var persons = CharterTestDataFixed.PersonsByEntity[charterNumber];

        // 1. Admin Console: создание ЮЛ + LE_ADMIN
        var adminPage = await CreateAdminConsolePageAsync();
        try
        {
            await CharterTestSeeder.EnsureSeededAsync(adminPage, charterNumber);
            // Логин SYS_ADMIN нужен для навигации (no-op сидирования не навигирует)
            if (!adminPage.Url.Contains("/access-management"))
            {
                await AuthHelper.LoginAsAdminAsync(adminPage, CharterTestDataFixed.SysAdminLogin);
            }
            await AdminConsoleHelper.NavigateToLegalEntityAsync(adminPage, entity.Name);

            // 2. Создание User для участника в Admin Console (нужен для BasicProvider)
            var p = persons.Participants[0];
            if (p.Login != entity.AdminUser.Login)
            {
                var nameParts = p.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (nameParts.Length >= 3)
                {
                    await AdminConsoleHelper.AddEmployeeAsync(
                        adminPage,
                        nameParts[0], nameParts[1], nameParts[2],
                        "Участник", p.Login,
                        CharterTestDataFixed.RoleLeAdmin);
                }
            }
        }
        finally
        {
            await adminPage.CloseAsync();
        }

        // 3. Board Portal: GD (LE_ADMIN) регистрирует участника с ПДн
        var gdLogin = persons.Gd?.Login ?? entity.AdminUser.Login;
        await AuthHelper.LoginAsBoardUserAsync(participantPage, gdLogin);
        participantPage.Url.Should().Contain("/main");

        // Поиск существующего EcosystemParticipant по ФИО (создан через Admin Console)
        var participantFullName = persons.Participants[0].FullName;
        var ecoId = await participantPage.EvaluateAsync<Guid?>(
            $@"async () => {{
                const response = await fetch('/api/participants/eco-search?name={Uri.EscapeDataString(participantFullName)}', {{
                    credentials: 'same-origin'
                }});
                if (!response.ok) return null;
                const data = await response.json();
                if (data && data.length > 0 && data[0].id) return data[0].id;
                return null;
            }}");

        // Уникальные ПДн на основе номера ЮЛ
        var passportSeries = (1000 + charterNumber).ToString();
        var passportNumber = (100000 + charterNumber * 111).ToString();
        var personInn = $"770{charterNumber:D5}000";

        var participantId = await BoardPortalHelper.AddParticipantWithPersonalDataAsync(
            participantPage,
            fullName: participantFullName,
            dulTypeCode: "21",
            passportSeries: passportSeries,
            passportNumber: passportNumber,
            personInn: personInn,
            participantType: "FL",
            sharePercent: persons.Participants[0].SharePercent,
            shareAmount: persons.Participants[0].SharePercent * 100m,
            ecosystemParticipantId: ecoId);

        participantId.Should().NotBeEmpty("участник должен быть создан");

        // 4. Ожидание ЕДИН binding → роль PARTICIPANT назначается автоматически
        await EdinTestHelper.WaitForEdinBindingAsync(participantPage, participantId, timeoutSeconds: 5);

        Console.WriteLine($"[US020] Участник {participantFullName} зарегистрирован (PARTICIPANT).");

        // 5. Выход GD — чтобы участник мог залогиниться
        await participantPage.GotoAsync(PortalUrls.GetUrl(Portal.BoardPortal, "/logout"));
        await AuthHelper.WaitForBlazorReady(participantPage);
    }
}
