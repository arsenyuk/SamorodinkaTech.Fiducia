using FluentAssertions;
using Microsoft.Playwright;

namespace SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

/// <summary>
/// Сидер базы данных для E2E-тестов уставов.
/// Выполняет сброс БД и создание всех ЮЛ + LDAP-пользователей ОДИН раз перед прогоном тестов.
/// Использует идемпотентную инициализацию: повторный вызов ничего не делает.
/// </summary>
public static class CharterTestSeeder
{
    private static bool _seeded;
    private static readonly SemaphoreSlim Semaphore = new(1, 1);

    /// <summary>
    /// Убедиться, что БД засеяна данными для всех уставов.
    /// Вызывать в начале каждого E2E-теста — повторные вызовы ничего не делают.
    /// </summary>
    public static async Task EnsureSeededAsync(IPage adminPage, IPage ldapPage)
    {
        if (_seeded) return;

        await Semaphore.WaitAsync();
        try
        {
            if (_seeded) return;

            await SeedAsync(adminPage, ldapPage);
            _seeded = true;
        }
        finally
        {
            Semaphore.Release();
        }
    }

    /// <summary>
    /// Полная процедура сидирования: сброс БД → LDAP → Admin Console → создание ЮЛ → роли → участники.
    /// </summary>
    private static async Task SeedAsync(IPage adminPage, IPage ldapPage)
    {
        // ═══════════════════════════════════════════════════════════════════
        // Шаг 1: Сброс БД
        // ═══════════════════════════════════════════════════════════════════
        await DbResetHelper.ResetAsync(includeDemo: false, timeout: TimeSpan.FromMinutes(3));

        // ═══════════════════════════════════════════════════════════════════
        // Шаг 2: Удаление старых LDAP-пользователей
        // ═══════════════════════════════════════════════════════════════════
        await LdapHelper.DeleteAllTestUsersAsync();

        // ═══════════════════════════════════════════════════════════════════
        // Шаг 3: Создание LDAP-пользователей для всех ЮЛ
        // ═══════════════════════════════════════════════════════════════════
        var allPersons = GetAllUniquePersons();

        foreach (var person in allPersons.Where(p => !string.IsNullOrEmpty(p.Uid)))
        {
            await LdapHelper.CreateUserAsync(
                ldapPage,
                person.Uid,
                person.FullName,
                person.LastName,
                person.FirstName,
                "test1234",
                addToBoardGroup: true);
        }

        // ═══════════════════════════════════════════════════════════════════
        // Шаг 4: Логин SYS_ADMIN в Admin Console
        // ═══════════════════════════════════════════════════════════════════
        await AuthHelper.LoginAsAdminAsync(adminPage, CharterTestDataFixed.SysAdminDisplayName);
        adminPage.Url.Should().Contain("/main");

        // ═══════════════════════════════════════════════════════════════════
        // Шаг 5: Переход в режим Пользователи
        // ═══════════════════════════════════════════════════════════════════
        await adminPage.GotoAsync(PortalUrls.GetUrl(Portal.AdminConsole, "/access-management"));
        await AuthHelper.WaitForBlazorReady(adminPage);
        await adminPage.WaitForTimeoutAsync(1000);

        // ═══════════════════════════════════════════════════════════════════
        // Шаг 6: Создание всех ЮЛ + назначение ролей
        // ═══════════════════════════════════════════════════════════════════
        foreach (var entity in CharterTestDataFixed.LegalEntities)
        {
            // Создание ЮЛ
            await AdminConsoleHelper.CreateLegalEntityAsync(adminPage, entity.Name, entity.Inn);

            // Назначение ролей ГД (для ExecutiveBody A) или первого участника (для B/C)
            var persons = CharterTestDataFixed.PersonsByEntity[entity.Number];

            if (persons.Gd is not null)
            {
                // ExecutiveBody A: ГД отдельно
                await AdminConsoleHelper.AssignRolesAsync(
                    adminPage,
                    persons.Gd.LastName,
                    persons.Gd.FirstName,
                    persons.Gd.MiddleName,
                    persons.Gd.Position,
                    persons.Gd.Uid,
                    [CharterTestDataFixed.RoleLeAdmin, CharterTestDataFixed.RoleCeo]);
            }
            else if (persons.Participants.Count > 0)
            {
                // ExecutiveBody B/C: первый участник = ГД (ЕИО)
                var firstParticipant = persons.Participants[0];
                var nameParts = firstParticipant.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (nameParts.Length >= 3)
                {
                    await AdminConsoleHelper.AssignRolesAsync(
                        adminPage,
                        nameParts[0], // Фамилия
                        nameParts[1], // Имя
                        nameParts[2], // Отчество
                        "Директор",
                        firstParticipant.FullName.ToLower().Replace(" ", "."),
                        [CharterTestDataFixed.RoleLeAdmin, CharterTestDataFixed.RoleCeo]);
                }
            }
        }
    }

    /// <summary>
    /// Получить уникальный список всех лиц из фиксированных данных.
    /// </summary>
    private static IEnumerable<CharterTestDataFixed.PersonData> GetAllUniquePersons()
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var entity in CharterTestDataFixed.LegalEntities)
        {
            if (!CharterTestDataFixed.PersonsByEntity.TryGetValue(entity.Number, out var persons))
                continue;

            // ГД (для ExecutiveBody A)
            if (persons.Gd is not null && seen.Add(persons.Gd.Uid))
            {
                yield return persons.Gd;
            }

            // Участники (с UID — только для B/C, где участники = ЕИО)
            foreach (var p in persons.Participants)
            {
                if (!string.IsNullOrEmpty(p.Uid) && seen.Add(p.Uid))
                {
                    yield return p;
                }
            }
        }
    }
}
