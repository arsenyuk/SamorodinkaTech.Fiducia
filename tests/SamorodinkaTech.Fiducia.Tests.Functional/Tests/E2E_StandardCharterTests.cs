using FluentAssertions;
using Microsoft.Playwright;
using SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

namespace SamorodinkaTech.Fiducia.Tests.Functional;

/// <summary>
/// Сквозные E2E-тесты для 36 типовых уставов ООО.
/// БД сбрасывается ОДИН раз перед прогоном всех тестов.
/// Каждый тест работает со своим фиксированным ЮЛ и набором лиц.
/// Запрещено параллельное исполнение (Collection "CharterTests").
/// Тестовый сценарий:
/// 1. Логин ГД в Board Portal (пользователь уже создан при сидировании)
/// 2. Заполнение полей ЮЛ + выбор типового устава
/// 3. Сохранение и проверка отсутствия ошибок
/// 4. Добавление участников (для ExecutiveBody A)
/// 5. Проверка записей аудита (вход, чтение/запись, участники)
/// 6. Проверка отсутствия ошибок в логе приложения за период работы теста
/// </summary>
[Collection("CharterTests")]
public class E2E_StandardCharterTests : BrowserFixture
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    [InlineData(11)]
    [InlineData(12)]
    [InlineData(13)]
    [InlineData(14)]
    [InlineData(15)]
    [InlineData(16)]
    [InlineData(17)]
    [InlineData(18)]
    [InlineData(19)]
    [InlineData(20)]
    [InlineData(21)]
    [InlineData(22)]
    [InlineData(23)]
    [InlineData(24)]
    [InlineData(25)]
    [InlineData(26)]
    [InlineData(27)]
    [InlineData(28)]
    [InlineData(29)]
    [InlineData(30)]
    [InlineData(31)]
    [InlineData(32)]
    [InlineData(33)]
    [InlineData(34)]
    [InlineData(35)]
    [InlineData(36)]
    public async Task StandardCharter_CompleteFlow(int charterNumber)
    {
        var testStartTime = DateTimeOffset.UtcNow;
        var testName = $"StandardCharter_CompleteFlow={charterNumber}";

        var adminPage = await CreateAdminConsolePageAsync();
        var boardPage = await CreateBoardPortalPageAsync();
        var ldapPage = await CreatePageAsync();

        try
        {
            // ═══════════════════════════════════════════════════════════════════
            // Идемпотентное сидирование: БД + LDAP + ЮЛ + роли (один раз)
            // ═══════════════════════════════════════════════════════════════════
            await CharterTestSeeder.EnsureSeededAsync(adminPage, ldapPage);

            // ═══════════════════════════════════════════════════════════════════
            // Получение фиксированных данных для данного устава
            // ═══════════════════════════════════════════════════════════════════
            var entity = CharterTestDataFixed.LegalEntities[charterNumber - 1];
            var persons = CharterTestDataFixed.PersonsByEntity[charterNumber];

            // ═══════════════════════════════════════════════════════════════════
            // Шаг 1: Логин ГД в Board Portal
            // ═══════════════════════════════════════════════════════════════════
            var gdDisplayName = persons.Gd?.FullName ?? persons.Participants[0].FullName;
            await AuthHelper.LoginAsBoardUserAsync(boardPage, gdDisplayName);
            boardPage.Url.Should().Contain("/main");

            // ═══════════════════════════════════════════════════════════════════
            // Шаг 2: Заполнение полей ЮЛ + выбор типового устава + сохранение
            // ═══════════════════════════════════════════════════════════════════
            await BoardPortalHelper.CompleteLegalEntitySetupAsync(
                boardPage,
                charterNumber,
                shortName: entity.ShortName,
                ogrn: entity.Ogrn);

            // ═══════════════════════════════════════════════════════════════════
            // Шаг 3: Проверка отсутствия ошибок
            // ═══════════════════════════════════════════════════════════════════
            var hasErrors = await boardPage.EvaluateAsync<bool>(
                "() => document.querySelectorAll('.alert-danger').length > 0");
            hasErrors.Should().BeFalse(
                $"Для типового устава №{charterNumber} не должно быть ошибок при сохранении");

            // ═══════════════════════════════════════════════════════════════════
            // Шаг 4: Добавление участников (только для ExecutiveBody A)
            // ═══════════════════════════════════════════════════════════════════
            if (entity.ExecutiveBodyType == CharterTestDataFixed.ExecutiveBodyA)
            {
                for (var i = 0; i < persons.Participants.Count; i++)
                {
                    var p = persons.Participants[i];
                    await BoardPortalHelper.AddParticipantAsync(
                        boardPage,
                        p.FullName,
                        sharePercent: p.SharePercent);
                }

                await BoardPortalHelper.AssertParticipantCountAsync(
                    boardPage,
                    persons.Participants.Count);
            }

            // ═══════════════════════════════════════════════════════════════════
            // Шаг 5: Проверка записей аудита
            // ═══════════════════════════════════════════════════════════════════
            var login = persons.Gd?.FullName ?? persons.Participants[0].FullName;

            // Вход в систему должен быть залогирован
            await AuditLogHelper.AssertLoginLoggedAsync(login);

            // Изменение данных ЮЛ (save) должно быть залогировано
            await AuditLogHelper.AssertDataUpdateLoggedAsync("legal-entities");

            // Добавление участников должно быть залогировано (только для типа A)
            if (entity.ExecutiveBodyType == CharterTestDataFixed.ExecutiveBodyA)
            {
                await AuditLogHelper.AssertDataCreateLoggedAsync("participants");
            }

            // Не должно быть ошибок доступа
            await AuditLogHelper.AssertNoAccessDeniedAsync();
        }
        finally
        {
            // ═══════════════════════════════════════════════════════════════════
            // Шаг 6: Проверка ошибок в логе приложения за период работы теста
            // ═══════════════════════════════════════════════════════════════════
            var testEndTime = DateTimeOffset.UtcNow;
            await AppLogHelper.AssertNoErrorsInAppLogSafeAsync(testStartTime, testEndTime, testName);

            await ldapPage.CloseAsync();
            await boardPage.CloseAsync();
            await adminPage.CloseAsync();
        }
    }
}
