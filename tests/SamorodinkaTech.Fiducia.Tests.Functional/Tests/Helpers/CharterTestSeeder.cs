using FluentAssertions;
using Microsoft.Playwright;

namespace SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

/// <summary>
/// Сидер базы данных для E2E-тестов уставов.
/// Создаёт ЮЛ и назначает роли через UI. Пользователи создаются автоматически
/// при первом входе через LDAP (auto-provisioning в LdapAuthProvider).
/// Использует per-entity идемпотентность: каждый ЮЛ создаётся один раз.
/// </summary>
public static class CharterTestSeeder
{
    private static bool _loggedIn;
    private static readonly HashSet<int> _seededEntities = [];
    private static Exception? _seedingException;
    private static readonly SemaphoreSlim Semaphore = new(1, 1);

    /// <summary>
    /// Сидировать указанное ЮЛ (один раз). Повторный вызов для того же ЮЛ — no-op.
    /// </summary>
    public static async Task<IPage?> EnsureSeededAsync(IPage adminPage, int charterNumber, IPage? ldapPage = null)
    {
        if (_seedingException is not null)
            throw new InvalidOperationException($"[Seeder] Сидирование завершилось ошибкой: {_seedingException.Message}", _seedingException);

        if (_seededEntities.Contains(charterNumber)) return adminPage;

        await Semaphore.WaitAsync();
        try
        {
            if (_seedingException is not null)
                throw new InvalidOperationException($"[Seeder] Сидирование завершилось ошибкой: {_seedingException.Message}", _seedingException);
            if (_seededEntities.Contains(charterNumber)) return adminPage;

            await SeedEntityAsync(adminPage, charterNumber);
            _seededEntities.Add(charterNumber);
            return adminPage;
        }
        catch (Exception ex)
        {
            _seedingException = ex;
            throw;
        }
        finally
        {
            Semaphore.Release();
        }
    }

    private static async Task SeedEntityAsync(IPage adminPage, int charterNumber)
    {
        // Логин SYS_ADMIN (один раз)
        if (!_loggedIn)
        {
            Console.WriteLine($"[Seeder] Логин SYS_ADMIN...");
            await AuthHelper.LoginAsAdminAsync(adminPage, CharterTestDataFixed.SysAdminLogin);
            adminPage.Url.Should().Contain("/main");
            _loggedIn = true;
            Console.WriteLine("[Seeder] Логин выполнен.");
        }

        var entity = CharterTestDataFixed.LegalEntities[charterNumber - 1];
        var persons = CharterTestDataFixed.PersonsByEntity[charterNumber];

        // ── Создание пользователей в БД ──────────────────────────────
        Console.WriteLine($"[Seeder] ЮЛ {charterNumber}: создание пользователей...");

        // Администратор
        var adminEmail = $"{entity.AdminUser.Login}@test.local";
        await AdminConsoleHelper.CreateUserViaUiAsync(
            adminPage, entity.AdminUser.Login,
            entity.AdminUser.LastName, entity.AdminUser.FirstName, entity.AdminUser.MiddleName, adminEmail);

        // ГД (или первый участник для типов B/C)
        if (persons.Gd is not null)
        {
            var email = $"{persons.Gd.Login}@test.local";
            await AdminConsoleHelper.CreateUserViaUiAsync(
                adminPage, persons.Gd.Login,
                persons.Gd.LastName, persons.Gd.FirstName, persons.Gd.MiddleName, email);
        }
        else if (persons.Participants.Count > 0)
        {
            var p = persons.Participants[0];
            var nameParts = p.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (nameParts.Length >= 3)
            {
                var email = $"{p.Login}@test.local";
                await AdminConsoleHelper.CreateUserViaUiAsync(
                    adminPage, p.Login, nameParts[0], nameParts[1], nameParts[2], email);
            }
        }

        // ── Создание ЮЛ + назначение ролей ───────────────────────────
        Console.WriteLine($"[Seeder] ЮЛ {charterNumber}: создание ЮЛ и назначение ролей...");
        await AdminConsoleHelper.NavigateToAsync(adminPage, "/access-management");

        Console.WriteLine($"[Seeder] ЮЛ {charterNumber}: {entity.Name} (ИНН {entity.Inn})...");
        await AdminConsoleHelper.CreateLegalEntityAsync(adminPage, entity.Name, entity.Inn);

        // Роли администратору (EcosystemParticipant + Employee + UserRole через UI)
        await AdminConsoleHelper.AssignRolesAsync(
            adminPage,
            entity.AdminUser.LastName, entity.AdminUser.FirstName, entity.AdminUser.MiddleName,
            entity.AdminUser.Position, entity.AdminUser.Login,
            [CharterTestDataFixed.RoleLeAdmin]);

        // Роли ГД (или первому участнику)
        if (persons.Gd is not null)
        {
            await AdminConsoleHelper.AssignRolesAsync(
                adminPage,
                persons.Gd.LastName, persons.Gd.FirstName, persons.Gd.MiddleName,
                persons.Gd.Position, persons.Gd.Login,
                [CharterTestDataFixed.RoleLeAdmin, CharterTestDataFixed.RoleCeo]);
        }
        else if (persons.Participants.Count > 0)
        {
            var p = persons.Participants[0];
            var nameParts = p.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (nameParts.Length >= 3)
            {
                await AdminConsoleHelper.AssignRolesAsync(
                    adminPage,
                    nameParts[0], nameParts[1], nameParts[2],
                    "Директор", p.Login,
                    [CharterTestDataFixed.RoleLeAdmin, CharterTestDataFixed.RoleCeo]);
            }
        }

        Console.WriteLine($"[Seeder] ЮЛ {charterNumber}: сидирование завершено.");
    }
}
