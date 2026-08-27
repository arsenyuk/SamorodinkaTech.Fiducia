using FluentAssertions;
using Microsoft.Playwright;

namespace SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

/// <summary>
/// Сидер базы данных для E2E-тестов уставов.
/// Создаёт ЮЛ и назначает роли через UI. Пользователи создаются автоматически
/// при первом входе через LDAP (auto-provisioning в LdapAuthProvider).
/// Использует идемпотентную инициализацию: повторный вызов ничего не делает.
/// </summary>
public static class CharterTestSeeder
{
    private static bool _seeded;
    private static Exception? _seedingException;
    private static readonly SemaphoreSlim Semaphore = new(1, 1);

    public static async Task<IPage?> EnsureSeededAsync(IPage adminPage, IPage ldapPage, Func<Task<IPage>>? createAdminPage = null)
    {
        if (_seedingException is not null)
            throw new InvalidOperationException($"[Seeder] Сидирование завершилось ошибкой: {_seedingException.Message}", _seedingException);

        if (_seeded) return adminPage;

        await Semaphore.WaitAsync();
        try
        {
            if (_seedingException is not null)
                throw new InvalidOperationException($"[Seeder] Сидирование завершилось ошибкой: {_seedingException.Message}", _seedingException);
            if (_seeded) return adminPage;

            var result = await SeedAsync(adminPage, ldapPage, createAdminPage);
            _seeded = true;
            return result;
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

    private static async Task<IPage> SeedAsync(IPage adminPage, IPage ldapPage, Func<Task<IPage>>? createAdminPage = null)
    {
        Console.WriteLine("[Seeder] Начало сидирования...");

        // Шаг 1: Логин SYS_ADMIN
        Console.WriteLine("[Seeder] Шаг 1: Логин SYS_ADMIN...");
        await AuthHelper.LoginAsAdminAsync(adminPage, CharterTestDataFixed.SysAdminLogin);
        adminPage.Url.Should().Contain("/main");
        Console.WriteLine("[Seeder] Шаг 1: Логин выполнен.");

        // Шаг 2: Создание пользователей в БД
        Console.WriteLine("[Seeder] Шаг 2: Создание пользователей...");
        foreach (var entity in CharterTestDataFixed.LegalEntities)
        {
            var persons = CharterTestDataFixed.PersonsByEntity[entity.Number];

            // Создание администратора ЮЛ
            var adminEmail = $"{entity.AdminUser.Login}@test.local";
            Console.WriteLine($"[Seeder] Пользователь администратора: {entity.AdminUser.FullName} ({entity.AdminUser.Login})...");
            await AdminConsoleHelper.CreateUserViaUiAsync(
                adminPage, entity.AdminUser.Login,
                entity.AdminUser.LastName, entity.AdminUser.FirstName, entity.AdminUser.MiddleName, adminEmail);

            // Создание ГД (или первого участника для типов B/C)
            if (persons.Gd is not null)
            {
                var email = $"{persons.Gd.Login}@test.local";
                Console.WriteLine($"[Seeder] Пользователь ГД: {persons.Gd.FullName} ({persons.Gd.Login})...");
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
                    Console.WriteLine($"[Seeder] Пользователь участника: {p.FullName} ({p.Login})...");
                    await AdminConsoleHelper.CreateUserViaUiAsync(
                        adminPage, p.Login, nameParts[0], nameParts[1], nameParts[2], email);
                }
            }
        }

        // Шаг 3: Создание ЮЛ + назначение ролей
        Console.WriteLine("[Seeder] Шаг 3: Создание ЮЛ и назначение ролей...");
        await adminPage.GotoAsync(PortalUrls.GetUrl(Portal.AdminConsole, "/access-management"));
        await AuthHelper.WaitForBlazorReady(adminPage);
        await adminPage.WaitForTimeoutAsync(1000);

        foreach (var entity in CharterTestDataFixed.LegalEntities)
        {
            Console.WriteLine($"[Seeder] Создание ЮЛ: {entity.Name} (ИНН {entity.Inn})...");
            await AdminConsoleHelper.CreateLegalEntityAsync(adminPage, entity.Name, entity.Inn);
            Console.WriteLine($"[Seeder] ЮЛ создано: {entity.Name}");

            var persons = CharterTestDataFixed.PersonsByEntity[entity.Number];

            // Назначение ролей администратору ЮЛ
            Console.WriteLine($"[Seeder] Назначение ролей администратору: {entity.AdminUser.FullName} ({entity.AdminUser.Login})...");
            await AdminConsoleHelper.AssignRolesAsync(
                adminPage,
                entity.AdminUser.LastName, entity.AdminUser.FirstName, entity.AdminUser.MiddleName,
                entity.AdminUser.Position, entity.AdminUser.Login,
                [CharterTestDataFixed.RoleLeAdmin]);
            Console.WriteLine($"[Seeder] Роли назначены: {entity.AdminUser.LastName}");

            // Назначение ролей ГД (или первому участнику)
            if (persons.Gd is not null)
            {
                Console.WriteLine($"[Seeder] Назначение ролей ГД: {persons.Gd.FullName} ({persons.Gd.Login})...");
                await AdminConsoleHelper.AssignRolesAsync(
                    adminPage,
                    persons.Gd.LastName, persons.Gd.FirstName, persons.Gd.MiddleName,
                    persons.Gd.Position, persons.Gd.Login,
                    [CharterTestDataFixed.RoleLeAdmin, CharterTestDataFixed.RoleCeo]);
                Console.WriteLine($"[Seeder] Роли назначены: {persons.Gd.LastName}");
            }
            else if (persons.Participants.Count > 0)
            {
                var p = persons.Participants[0];
                var nameParts = p.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (nameParts.Length >= 3)
                {
                    Console.WriteLine($"[Seeder] Назначение ролей участника: {p.FullName} ({p.Login})...");
                    await AdminConsoleHelper.AssignRolesAsync(
                        adminPage,
                        nameParts[0], nameParts[1], nameParts[2],
                        "Директор", p.Login,
                        [CharterTestDataFixed.RoleLeAdmin, CharterTestDataFixed.RoleCeo]);
                    Console.WriteLine($"[Seeder] Роли назначены: {p.FullName}");
                }
            }
        }

        Console.WriteLine("[Seeder] Сидирование завершено.");
        return adminPage;
    }
}
