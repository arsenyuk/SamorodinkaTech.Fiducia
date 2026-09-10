using FluentAssertions;
using Microsoft.Playwright;
using SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

namespace SamorodinkaTech.Fiducia.Tests.Functional;

/// <summary>
/// Сквозной E2E-тест: требование участника о созыве ВОСУ.
/// Паттерн: Admin Console (User + EcoParticipant + Role) →
///          Board Portal (BoardParticipant с ДУЛ) →
///          ЕДИН binding (MasterId → привязка) →
///          подача DEMAND_VOSU → решение CEO → план ВОСУ.
///
/// Роль PARTICIPANT назначается АВТОМАТИЧЕСКИ при привязке
/// BoardParticipant к EcosystemParticipant через ЕДИН binding.
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

        var (adminPage, boardPage, ldapPage) = await SetupFullCycleAsync(67);
        try
        {
            var persons = CharterTestDataFixed.PersonsByEntity[67];
            var gdLogin = persons.Gd?.Login!;
            var participantLogin = persons.Participants[0].Login;
            var participantFullName = persons.Participants[0].FullName;

            // ── Шаг 1: ГД добавляет BoardParticipant для участника ──────
            // ГД (ivanov.tm) залогинен после SetupFullCycle
            await AuthHelper.LoginAsBoardUserAsync(boardPage, gdLogin);
            boardPage.Url.Should().Contain("/main");

            var participantId = await BoardPortalHelper.AddParticipantWithPersonalDataAsync(
                boardPage,
                fullName: participantFullName,
                passportSeries: "21",
                passportNumber: "4690",
                personInn: "781234567890",
                participantType: "FL",
                sharePercent: 40m,
                shareAmount: 40000m);

            participantId.Should().NotBeEmpty("участник должен быть создан");

            // ── Шаг 2: Ожидание ЕДИН binding ──────────────────────────
            // Роль PARTICIPANT назначается автоматически при привязке
            await EdinTestHelper.WaitForEdinBindingAsync(boardPage, participantId, timeoutSeconds: 15);

            var mpiMasterId = await EdinTestHelper.GetParticipantMpiMasterIdAsync(boardPage, participantId);
            mpiMasterId.Should().NotBeNull("ЕДИН должен привязать MasterId");

            // ── Шаг 3: Участник подаёт требование DEMAND_VOSU ──────────
            await AuthHelper.LoginAsBoardUserAsync(boardPage, participantLogin);
            boardPage.Url.Should().Contain("/main");

            // Навигация через UI: "Мои запросы"
            await boardPage.ClickAsync("text=Мои запросы");
            await AuthHelper.WaitForBlazorReady(boardPage);
            await boardPage.WaitForTimeoutAsync(2000);

            // Кликаем "Подать требование"
            await boardPage.ClickAsync("text=Подать требование");
            await AuthHelper.WaitForBlazorReady(boardPage);
            await boardPage.WaitForTimeoutAsync(2000);

            // Выбираем тип DEMAND_VOSU
            await boardPage.ClickAsync("text=Требование о созыве ВОСУ");
            await boardPage.WaitForTimeoutAsync(1000);

            // Заполняем текст требования
            var textarea = await boardPage.WaitForSelectorAsync("textarea", new() { Timeout = 10000 });
            await textarea!.FillAsync("Требование о созыве внеочередного общего собрания участников для рассмотрения вопроса о смене генерального директора");

            // Ставим галочку
            var checkbox = await boardPage.WaitForSelectorAsync("#agreeWarning", new() { Timeout = 5000 });
            await checkbox!.ClickAsync();
            await boardPage.WaitForTimeoutAsync(500);

            // Отправляем
            await boardPage.ClickAsync("button:text('Подать требование')");
            await boardPage.WaitForTimeoutAsync(3000);

            boardPage.Url.Should().Contain("/share-requests");

            // ── Шаг 4: CEO принимает требование ───────────────────────
            await AuthHelper.LoginAsBoardUserAsync(boardPage, gdLogin);
            boardPage.Url.Should().Contain("/main");

            await boardPage.ClickAsync("text=Оповещения");
            await AuthHelper.WaitForBlazorReady(boardPage);
            await boardPage.WaitForTimeoutAsync(2000);

            var notification = await boardPage.WaitForSelectorAsync(
                "text=Требование участника о созыве ВОСУ",
                new() { Timeout = 10000 });
            notification.Should().NotBeNull("уведомление должно отображаться");

            // Кликаем по ссылке уведомления
            var link = await boardPage.WaitForSelectorAsync(
                "a:text('Требование участника о созыве ВОСУ')",
                new() { Timeout = 5000 });
            if (link is not null)
            {
                await link.ClickAsync();
                await AuthHelper.WaitForBlazorReady(boardPage);
                await boardPage.WaitForTimeoutAsync(3000);
            }
            else
            {
                await boardPage.ClickAsync("text=Требования участников");
                await AuthHelper.WaitForBlazorReady(boardPage);
                await boardPage.WaitForTimeoutAsync(2000);
                var row = await boardPage.WaitForSelectorAsync("table tbody tr", new() { Timeout = 5000 });
                if (row is not null)
                {
                    await row.ClickAsync();
                    await AuthHelper.WaitForBlazorReady(boardPage);
                    await boardPage.WaitForTimeoutAsync(3000);
                }
            }

            await boardPage.WaitForSelectorAsync("text=Дедлайн решения", new() { Timeout = 10000 });

            boardPage.Dialog += async (_, dialog) => await dialog.AcceptAsync();
            await boardPage.ClickAsync("button:text('Принять требование')");
            await boardPage.WaitForTimeoutAsync(5000);

            await boardPage.WaitForSelectorAsync("text=Перейти к плану ВОСУ", new() { Timeout = 10000 });
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

    private async Task<(IPage adminPage, IPage boardPage, IPage ldapPage)> SetupFullCycleAsync(int entityIndex)
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

        return (adminPage, boardPage, ldapPage);
    }

    private static async Task CleanupAsync(IPage adminPage, IPage boardPage, IPage ldapPage)
    {
        await ldapPage.CloseAsync();
        await boardPage.CloseAsync();
        await adminPage.CloseAsync();
    }
}
