using FluentAssertions;
using Microsoft.Playwright;
using SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

namespace SamorodinkaTech.Fiducia.Tests.Functional;

/// <summary>
/// US-023: Участники ООО — E2E-тест через Playwright.
/// Сценарий: участник (PARTICIPANT) заходит на страницу участников общества,
/// проверяет загрузку, вкладки, API и отсутствие ошибок.
/// Страница доступна только для ООО (ОКОПФ 12300).
/// Документация: docs/e2e-participant-list.md
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
            await SetupParticipantAsync(page, 1);
            await AuthHelper.LoginAsBoardUserAsync(page, "zhirov.at");

            await page.GotoAsync(PortalUrls.GetUrl(Portal.BoardPortal, "/participants"));
            await AuthHelper.WaitForBlazorReady(page);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await page.WaitForSelectorAsync("h3", new() { Timeout = DefaultTimeout });

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
            await SetupParticipantAsync(page, 1);
            await AuthHelper.LoginAsBoardUserAsync(page, "zhirov.at");

            await page.GotoAsync(PortalUrls.GetUrl(Portal.BoardPortal, "/participants"));
            await AuthHelper.WaitForBlazorReady(page);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await page.WaitForSelectorAsync("h3", new() { Timeout = DefaultTimeout });

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
            await SetupParticipantAsync(page, 1);
            await AuthHelper.LoginAsBoardUserAsync(page, "zhirov.at");

            await page.GotoAsync(PortalUrls.GetUrl(Portal.BoardPortal, "/participants"));
            await AuthHelper.WaitForBlazorReady(page);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await page.WaitForSelectorAsync("h3", new() { Timeout = DefaultTimeout });

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
            await SetupParticipantAsync(page, 1);
            await AuthHelper.LoginAsBoardUserAsync(page, "zhirov.at");

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
            await SetupParticipantAsync(page, 1);
            await AuthHelper.LoginAsBoardUserAsync(page, "zhirov.at");

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

    /// <summary>
    /// US-023: Страница карточки участника загружается и содержит данные.
    /// </summary>
    [Fact]
    public async Task BoardPortal_ParticipantDetail_ShouldLoadWithAllFields()
    {
        SkipIfPreviousFailed();

        var page = await CreateBoardPortalPageAsync();
        try
        {
            await SetupParticipantAsync(page, 1);

            // Логин как GD (LE_ADMIN) для доступа к карточке участника
            var gdLogin = CharterTestDataFixed.PersonsByEntity[1].Gd?.Login
                          ?? CharterTestDataFixed.LegalEntities[0].AdminUser.Login;
            await AuthHelper.LoginAsBoardUserAsync(page, gdLogin);

            // Получаем ID первого участника через API
            var participantId = await page.EvaluateAsync<Guid?>(
                @"async () => {
                    const resp = await fetch('/api/participants', {
                        method: 'GET',
                        credentials: 'same-origin'
                    });
                    if (!resp.ok) return null;
                    const data = await resp.json();
                    if (data && data.length > 0 && data[0].id) return data[0].id;
                    return null;
                }");

            participantId.Should().NotBeNull("участник должен существовать");

            // Навигация на страницу карточки участника
            await page.GotoAsync(PortalUrls.GetUrl(Portal.BoardPortal, $"/participants/{participantId}"));
            await AuthHelper.WaitForBlazorReady(page);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await page.WaitForSelectorAsync("h3", new() { Timeout = DefaultTimeout });

            var content = await page.ContentAsync();

            content.Should().Contain("_framework/blazor.server.js",
                "Board Portal: ParticipantDetail должен содержать Blazor shell");

            content.Should().Contain("Основные сведения",
                "Board Portal: ParticipantDetail должен содержать секцию «Основные сведения»");

            content.Should().Contain("Документы",
                "Board Portal: ParticipantDetail должен содержать секцию «Документы»");

            content.Should().Contain("Привязка к ЕДИН",
                "Board Portal: ParticipantDetail должен содержать секцию «Привязка к ЕДИН»");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    /// <summary>
    /// US-023: Участник типа ЮЛ создаётся и отображается в карточке.
    /// </summary>
    [Fact]
    public async Task BoardPortal_ParticipantUl_ShouldCreateAndShowDetail()
    {
        SkipIfPreviousFailed();

        var page = await CreateBoardPortalPageAsync();
        try
        {
            await SetupParticipantAsync(page, 1);

            // Логин как GD (LE_ADMIN)
            var gdLogin = CharterTestDataFixed.PersonsByEntity[1].Gd?.Login
                          ?? CharterTestDataFixed.LegalEntities[0].AdminUser.Login;
            await AuthHelper.LoginAsBoardUserAsync(page, gdLogin);

            // Создаём участника типа ЮЛ
            var ulId = await BoardPortalHelper.AddParticipantUlAsync(
                page,
                companyName: "ООО «Тестовый поставщик»",
                companyInn: "7709998880",
                companyOgrn: "1157799001234",
                companyKpp: "770901001",
                companyAddress: "г. Москва, ул. Тестовая, д. 1",
                sharePercent: 25m,
                shareAmount: 25000m);

            ulId.Should().NotBeEmpty("участник ЮЛ должен быть создан");

            // Навигация на карточку ЮЛ-участника
            await page.GotoAsync(PortalUrls.GetUrl(Portal.BoardPortal, $"/participants/{ulId}"));
            await AuthHelper.WaitForBlazorReady(page);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await page.WaitForSelectorAsync("h3", new() { Timeout = DefaultTimeout });

            var content = await page.ContentAsync();

            content.Should().Contain("Основные сведения",
                "Карточка ЮЛ-участника должна содержать секцию «Основные сведения»");

            content.Should().Contain("Реквизиты юридического лица",
                "Карточка ЮЛ-участника должна содержать секцию «Реквизиты юридического лица»");

            content.Should().Contain("Тестовый поставщик",
                "Карточка ЮЛ-участника должна отображать наименование компании");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    /// <summary>
    /// US-023: Создание участника ФЛ без фамилии — сервер возвращает 400.
    /// </summary>
    [Fact]
    public async Task BoardPortal_ParticipantFl_EmptyLastName_ShouldReject()
    {
        SkipIfPreviousFailed();

        var page = await CreateBoardPortalPageAsync();
        try
        {
            await SetupParticipantAsync(page, 1);
            var gdLogin = CharterTestDataFixed.PersonsByEntity[1].Gd?.Login
                          ?? CharterTestDataFixed.LegalEntities[0].AdminUser.Login;
            await AuthHelper.LoginAsBoardUserAsync(page, gdLogin);

            var statusCode = await page.EvaluateAsync<int>(
                @"async () => {
                    const resp = await fetch('/api/participants', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        credentials: 'same-origin',
                        body: JSON.stringify({
                            participantType: 'FL',
                            lastName: '',
                            firstName: 'Тест',
                            sharePercent: 50,
                            paymentInfo: 'Оплачено'
                        })
                    });
                    return resp.status;
                }");

            statusCode.Should().Be(400, "Создание ФЛ без фамилии должно возвращать 400");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    /// <summary>
    /// US-023: Создание участника ФЛ без имени — сервер возвращает 400.
    /// </summary>
    [Fact]
    public async Task BoardPortal_ParticipantFl_EmptyFirstName_ShouldReject()
    {
        SkipIfPreviousFailed();

        var page = await CreateBoardPortalPageAsync();
        try
        {
            await SetupParticipantAsync(page, 1);
            var gdLogin = CharterTestDataFixed.PersonsByEntity[1].Gd?.Login
                          ?? CharterTestDataFixed.LegalEntities[0].AdminUser.Login;
            await AuthHelper.LoginAsBoardUserAsync(page, gdLogin);

            var statusCode = await page.EvaluateAsync<int>(
                @"async () => {
                    const resp = await fetch('/api/participants', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        credentials: 'same-origin',
                        body: JSON.stringify({
                            participantType: 'FL',
                            lastName: 'Тестов',
                            firstName: '',
                            sharePercent: 50,
                            paymentInfo: 'Оплачено'
                        })
                    });
                    return resp.status;
                }");

            statusCode.Should().Be(400, "Создание ФЛ без имени должно возвращать 400");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    /// <summary>
    /// US-023: Создание участника без оплаты доли — сервер возвращает 400.
    /// </summary>
    [Fact]
    public async Task BoardPortal_ParticipantFl_EmptyPayment_ShouldReject()
    {
        SkipIfPreviousFailed();

        var page = await CreateBoardPortalPageAsync();
        try
        {
            await SetupParticipantAsync(page, 1);
            var gdLogin = CharterTestDataFixed.PersonsByEntity[1].Gd?.Login
                          ?? CharterTestDataFixed.LegalEntities[0].AdminUser.Login;
            await AuthHelper.LoginAsBoardUserAsync(page, gdLogin);

            var statusCode = await page.EvaluateAsync<int>(
                @"async () => {
                    const resp = await fetch('/api/participants', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        credentials: 'same-origin',
                        body: JSON.stringify({
                            participantType: 'FL',
                            lastName: 'Тестов',
                            firstName: 'Тест',
                            sharePercent: 50,
                            paymentInfo: ''
                        })
                    });
                    return resp.status;
                }");

            statusCode.Should().Be(400, "Создание участника без оплаты доли должно возвращать 400");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    /// <summary>
    /// US-023: Участник с долей 100% без сведений об оплате — допустимо (201).
    /// </summary>
    [Fact]
    public async Task BoardPortal_ParticipantFl_FullyPaidNoPayment_ShouldAccept()
    {
        SkipIfPreviousFailed();

        var page = await CreateBoardPortalPageAsync();
        try
        {
            await SetupParticipantAsync(page, 1);
            var gdLogin = CharterTestDataFixed.PersonsByEntity[1].Gd?.Login
                          ?? CharterTestDataFixed.LegalEntities[0].AdminUser.Login;
            await AuthHelper.LoginAsBoardUserAsync(page, gdLogin);

            var statusCode = await page.EvaluateAsync<int>(
                @"async () => {
                    const resp = await fetch('/api/participants', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        credentials: 'same-origin',
                        body: JSON.stringify({
                            participantType: 'FL',
                            lastName: 'Полный',
                            firstName: 'Оплата',
                            sharePercent: 100,
                            paymentInfo: ''
                        })
                    });
                    return resp.status;
                }");

            statusCode.Should().Be(201, "Участник с долей 100% без сведений об оплате должен создаваться");
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
