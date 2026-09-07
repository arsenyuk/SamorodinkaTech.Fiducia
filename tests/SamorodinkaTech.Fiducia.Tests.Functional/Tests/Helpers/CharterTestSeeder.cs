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
        // Логин SYS_ADMIN: проверяем текущую страницу, а не статический флаг
        if (!adminPage.Url.Contains("/main"))
        {
            Console.WriteLine($"[Seeder] Логин SYS_ADMIN...");
            await AuthHelper.LoginAsAdminAsync(adminPage, CharterTestDataFixed.SysAdminLogin);
            adminPage.Url.Should().Contain("/main");
            Console.WriteLine("[Seeder] Логин выполнен.");
        }

        var entity = CharterTestDataFixed.LegalEntities[charterNumber - 1];
        var persons = CharterTestDataFixed.PersonsByEntity[charterNumber];

        // ── Создание ЮЛ + назначение ролей ───────────────────────────
        Console.WriteLine($"[Seeder] ЮЛ {charterNumber}: создание ЮЛ...");
        await AdminConsoleHelper.NavigateToAsync(adminPage, "/access-management");

        Console.WriteLine($"[Seeder] ЮЛ {charterNumber}: {entity.Name} (ИНН {entity.Inn})...");
        await AdminConsoleHelper.CreateLegalEntityAsync(adminPage, entity.Name, entity.Inn);

        // ── Установка ОКОПФ (ООО = 12300) ──────────────────────────
        var selectedLeId = await adminPage.EvaluateAsync<string?>(
            @"() => {
                const sel = document.querySelector('.card-body select.form-select');
                return sel ? sel.value : null;
            }");
        if (!string.IsNullOrEmpty(selectedLeId) && Guid.TryParse(selectedLeId, out var leGuid))
        {
            Console.WriteLine($"[Seeder] ЮЛ {charterNumber}: установка ОКОПФ 12300 (ООО)...");
            await AdminConsoleHelper.SetOkopfAsync(adminPage, leGuid, "12300");
        }

        // ── Добавление сотрудников (User + EcosystemParticipant + Employee) ──
        Console.WriteLine($"[Seeder] ЮЛ {charterNumber}: добавление сотрудников...");

        // Администратор
        await AdminConsoleHelper.AddEmployeeAsync(
            adminPage,
            entity.AdminUser.LastName, entity.AdminUser.FirstName, entity.AdminUser.MiddleName,
            entity.AdminUser.Position, entity.AdminUser.Login,
            CharterTestDataFixed.RoleLeAdmin);

        // ГД (или первый участник для типов B/C)
        if (persons.Gd is not null)
        {
            // Пропускаем если ГД = администратор (тот же login — один человек)
            if (persons.Gd.Login != entity.AdminUser.Login)
            {
                await AdminConsoleHelper.AddEmployeeAsync(
                    adminPage,
                    persons.Gd.LastName, persons.Gd.FirstName, persons.Gd.MiddleName,
                    persons.Gd.Position, persons.Gd.Login,
                    CharterTestDataFixed.RoleCeo);
            }
        }
        else if (persons.Participants.Count > 0)
        {
            var p = persons.Participants[0];
            // Пропускаем если участник = администратор (тот же login — один человек)
            if (p.Login != entity.AdminUser.Login)
            {
                var nameParts = p.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (nameParts.Length >= 3)
                {
                    await AdminConsoleHelper.AddEmployeeAsync(
                        adminPage,
                        nameParts[0], nameParts[1], nameParts[2],
                        "Директор", p.Login,
                        CharterTestDataFixed.RoleCeo);
                }
            }
        }

        Console.WriteLine($"[Seeder] ЮЛ {charterNumber}: сидирование завершено.");
    }
}
