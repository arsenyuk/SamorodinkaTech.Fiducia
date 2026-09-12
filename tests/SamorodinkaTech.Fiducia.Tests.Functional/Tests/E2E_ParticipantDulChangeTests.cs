using FluentAssertions;
using Microsoft.Playwright;
using SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

namespace SamorodinkaTech.Fiducia.Tests.Functional;

/// <summary>
/// Сквозной E2E-тест: изменение сведений участника (ДУЛ) с версионированием.
/// Сценарий:
/// 1. ГД добавляет участника с ДУЛ
/// 2. Участник подаёт новую запись сведений (новый паспорт)
/// 3. Новая запись автоматически применяется (версионирование ДУЛ)
/// 4. Проверка: 2 версии ДУЛ (старая неактивна, новая активна)
/// </summary>
[Collection("CharterTests")]
public class E2E_ParticipantDulChangeTests : BrowserFixture
{
    public E2E_ParticipantDulChangeTests(GlobalFixture globalFixture) : base(globalFixture)
    {
    }

    [Fact]
    public async Task DulChange_ParticipantUpdatesPassport_ShouldVersionDocument()
    {
        SkipIfPreviousFailed();

        var testStartTime = DateTimeOffset.UtcNow;
        var testName = "DulChange_ParticipantUpdatesPassport";

        var (adminPage, boardPage, ldapPage) = await SetupFullCycleAsync(68);
        try
        {
            var persons = CharterTestDataFixed.PersonsByEntity[68];
            var gdLogin = persons.Gd?.Login!;
            var participantLogin = persons.Participants[0].Login;
            var participantFullName = persons.Participants[0].FullName;

            // ── Шаг 1: ГД добавляет участника с ДУЛ ──────────────
            await AuthHelper.LoginAsBoardUserAsync(boardPage, gdLogin);
            boardPage.Url.Should().Contain("/main");

            var participantId = await BoardPortalHelper.AddParticipantWithDulAsync(
                boardPage,
                fullName: participantFullName,
                dulTypeCode: "21",
                dulSeries: "4600",
                dulNumber: "111222",
                personInn: "781234567890",
                sharePercent: 60m);

            participantId.Should().NotBeEmpty("участник должен быть создан");

            // ── Шаг 2: Проверяем начальное состояние ──────────────
            var docsBefore = await BoardPortalHelper.GetAllIdentityDocumentsAsync(boardPage, participantId);
            docsBefore.Should().HaveCount(1, "должна быть одна версия ДУЛ");
            docsBefore[0].IsActive.Should().BeTrue("версия должна быть активной");
            docsBefore[0].Series.Should().Be("4600");
            docsBefore[0].Number.Should().Be("111222");

            // ── Шаг 3: Участник подаёт новую запись сведений ─────
            await AuthHelper.LoginAsBoardUserAsync(boardPage, participantLogin);
            boardPage.Url.Should().Contain("/main");

            var changeId = await BoardPortalHelper.SubmitParticipantChangeAsync(
                boardPage,
                participantId: participantId,
                dulTypeCode: "21",
                passportSeries: "4610",
                passportNumber: "333444",
                comment: "Замена паспорта в связи с окончанием срока действия");
            changeId.Should().NotBeEmpty("информирование должно быть создано");

            // ── Шаг 4: Проверяем результат версионирования ───────
            var docsAfter = await BoardPortalHelper.GetAllIdentityDocumentsAsync(boardPage, participantId);
            docsAfter.Should().HaveCount(2, "должны быть 2 версии ДУЛ");

            // Старая версия — неактивна
            var oldDoc = docsAfter.Single(d => d.Series == "4600" && d.Number == "111222");
            oldDoc.IsActive.Should().BeFalse("старая версия должна стать неактивной");

            // Новая версия — активна
            var newDoc = docsAfter.Single(d => d.Series == "4610" && d.Number == "333444");
            newDoc.IsActive.Should().BeTrue("новая версия должна быть активной");

            // GET /api/participants/{id} показывает новый паспорт
            var participantAfter = await BoardPortalHelper.GetParticipantAsync(boardPage, participantId);
            participantAfter.Should().NotBeNull();
            participantAfter!.DulSeries.Should().Be("4610");
            participantAfter.DulNumber.Should().Be("333444");

            // ── Шаг 5: Проверка аудита ───────────────────────────
            await AuditLogHelper.AssertLoginLoggedAsync(gdLogin);
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
