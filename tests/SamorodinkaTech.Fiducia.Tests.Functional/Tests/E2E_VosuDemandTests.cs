using FluentAssertions;
using Microsoft.Playwright;
using SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

namespace SamorodinkaTech.Fiducia.Tests.Functional;

/// <summary>
/// Сквозной E2E-тест: требование участника о созыве ВОСУ.
/// Сценарий:
/// 1. Участник с долей ≥10% подаёт требование DEMAND_VOSU
/// 2. Требование сразу направляется ГД (SUBMITTED_TO_CEO)
/// 3. Участник получает уведомление "Требование направлено ГД"
/// 4. ГД видит уведомление и открывает страницу требований
/// 5. ГД принимает требование → создаётся план ВОСУ
/// </summary>
[Collection("CharterTests")]
public class E2E_VosuDemandTests : BrowserFixture
{
    public E2E_VosuDemandTests(GlobalFixture globalFixture) : base(globalFixture)
    {
    }

    [Fact]
    public async Task VosuDemand_ParticipantWithSufficientShare_ShouldReachCeoAndCreatePlan()
    {
        SkipIfPreviousFailed();

        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "VosuDemand_ParticipantWithSufficientShare";

        var (adminPage, boardPage, ldapPage, login) = await SetupFullCycleAsync(67);
        try
        {
            var entityIndex = 67;
            var persons = CharterTestDataFixed.PersonsByEntity[entityIndex];
            var gdLogin = persons.Gd?.Login ?? persons.Participants[0].Login;
            var participantLogin = persons.Participants[0].Login;

            // ── Часть А: Участник подаёт требование ──────────────────────
            // Входим как участник (не ГД)
            await AuthHelper.LoginAsBoardUserAsync(boardPage, participantLogin);
            boardPage.Url.Should().Contain("/main");

            // Переходим на страницу создания требования
            await boardPage.GotoAsync(PortalUrls.GetUrl(Portal.BoardPortal, "/share-requests/create"));
            await AuthHelper.WaitForBlazorReady(boardPage);
            await boardPage.WaitForTimeoutAsync(2000);

            // Проверяем, что страница загрузилась
            var h3 = await boardPage.WaitForSelectorAsync("h3", new() { Timeout = 15000 });
            h3.Should().NotBeNull();

            // Выбираем тип DEMAND_VOSU
            await boardPage.WaitForSelectorAsync("text=Требование о созыве ВОСУ", new() { Timeout = 10000 });
            await boardPage.ClickAsync("text=Требование о созыве ВОСУ");
            await boardPage.WaitForTimeoutAsync(1000);

            // Заполняем текст требования
            var textarea = await boardPage.WaitForSelectorAsync("textarea", new() { Timeout = 5000 });
            await textarea!.FillAsync("Требование о созыве внеочередного общего собрания участников для рассмотрения вопроса о смене генерального директора");

            // Ставим галочку "ознакомлен с рекомендациями"
            var checkbox = await boardPage.WaitForSelectorAsync("#agreeWarning", new() { Timeout = 5000 });
            await checkbox!.ClickAsync();
            await boardPage.WaitForTimeoutAsync(500);

            // Отправляем требование
            await boardPage.ClickAsync("button:text('Подать требование')");
            await boardPage.WaitForTimeoutAsync(3000);

            // Проверяем редирект на список требований
            boardPage.Url.Should().Contain("/share-requests");

            // ── Часть Б: ГД проверяет уведомление и принимает ────────────
            // Входим как ГД
            await AuthHelper.LoginAsBoardUserAsync(boardPage, gdLogin);
            boardPage.Url.Should().Contain("/main");

            // Проверяем уведомление
            await boardPage.GotoAsync(PortalUrls.GetUrl(Portal.BoardPortal, "/notifications"));
            await AuthHelper.WaitForBlazorReady(boardPage);
            await boardPage.WaitForTimeoutAsync(2000);

            // Ищем уведомление о требовании
            var notification = await boardPage.WaitForSelectorAsync(
                "text=Требование участника о созыве ВОСУ",
                new() { Timeout = 10000 });
            notification.Should().NotBeNull("Уведомление о требовании должно отображаться");

            // Кликаем по ссылке уведомления (переход на /ceo-demands/{id})
            var notificationLink = await boardPage.WaitForSelectorAsync(
                "a:text('Требование участника о созыве ВОСУ')",
                new() { Timeout = 5000 });
            if (notificationLink is not null)
            {
                await notificationLink.ClickAsync();
                await boardPage.WaitForTimeoutAsync(3000);
            }
            else
            {
                // Если ссылки нет — переходим через меню
                await boardPage.GotoAsync(PortalUrls.GetUrl(Portal.BoardPortal, "/ceo-demands"));
                await AuthHelper.WaitForBlazorReady(boardPage);
                await boardPage.WaitForTimeoutAsync(2000);
            }

            // Проверяем страницу CEO: либо список, либо детали
            if (boardPage.Url.Contains("/ceo-demands/"))
            {
                // Мы на странице деталей — проверяем индикатор дедлайна
                await boardPage.WaitForSelectorAsync("text=Дедлайн решения", new() { Timeout = 5000 });

                // Нажимаем "Принять"
                await boardPage.ClickAsync("button:text('Принять требование')");
                await boardPage.WaitForTimeoutAsync(2000);

                // Подтверждаем в диалоге
                boardPage.Dialog += async (_, dialog) => await dialog.AcceptAsync();
                await boardPage.WaitForTimeoutAsync(3000);

                // Проверяем ссылку на план
                await boardPage.WaitForSelectorAsync("text=Перейти к плану ВОСУ", new() { Timeout = 10000 });
            }
            else if (boardPage.Url.Contains("/ceo-demands"))
            {
                // Мы на странице списка — кликаем по первому требованию
                var demandRow = await boardPage.WaitForSelectorAsync("table tbody tr", new() { Timeout = 5000 });
                if (demandRow is not null)
                {
                    await demandRow.ClickAsync();
                    await boardPage.WaitForTimeoutAsync(3000);

                    // Проверяем индикатор дедлайна
                    await boardPage.WaitForSelectorAsync("text=Дедлайн решения", new() { Timeout = 5000 });

                    // Нажимаем "Принять"
                    await boardPage.ClickAsync("button:text('Принять требование')");
                    await boardPage.WaitForTimeoutAsync(2000);

                    // Подтверждаем в диалоге
                    boardPage.Dialog += async (_, dialog) => await dialog.AcceptAsync();
                    await boardPage.WaitForTimeoutAsync(3000);

                    // Проверяем ссылку на план
                    await boardPage.WaitForSelectorAsync("text=Перейти к плану ВОСУ", new() { Timeout = 10000 });
                }
            }
        }
        catch
        {
            GlobalFixture.MarkFailed();
            throw;
        }
        finally
        {
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);
            await CleanupAsync(adminPage, boardPage, ldapPage);
        }
    }

    private async Task<(IPage adminPage, IPage boardPage, IPage ldapPage, string login)> SetupFullCycleAsync(int entityIndex)
    {
        await InfrastructureHelper.EnsureInfrastructureReadyAsync();

        var adminPage = await CreateAdminConsolePageAsync();
        var boardPage = await CreateBoardPortalPageAsync();
        var ldapPage = await CreatePageAsync();

        await CharterTestGlobalInit.InitializeAsync(adminPage, ldapPage);
        await CharterTestSeeder.EnsureSeededAsync(adminPage, entityIndex);

        var entity = CharterTestDataFixed.LegalEntities[entityIndex - 1];
        var persons = CharterTestDataFixed.PersonsByEntity[entityIndex];

        var gdLogin = persons.Gd?.Login ?? persons.Participants[0].Login;
        await AuthHelper.LoginAsBoardUserAsync(boardPage, gdLogin);
        boardPage.Url.Should().Contain("/main");

        await BoardPortalHelper.FillLegalEntityFieldsAsync(
            boardPage,
            shortName: entity.ShortName,
            ogrn: entity.Ogrn);

        return (adminPage, boardPage, ldapPage, gdLogin);
    }

    private static async Task CleanupAsync(IPage adminPage, IPage boardPage, IPage ldapPage)
    {
        await ldapPage.CloseAsync();
        await boardPage.CloseAsync();
        await adminPage.CloseAsync();
    }
}
