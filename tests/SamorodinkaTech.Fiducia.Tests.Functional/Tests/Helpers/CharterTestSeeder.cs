using FluentAssertions;
using Microsoft.Playwright;

namespace SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

/// <summary>
/// Сидер базы данных для E2E-тестов уставов.
/// Создаёт ЮЛ и LE_ADMIN в Admin Console.
/// Регистрация участников (PARTICIPANT) выполняется тестами через Board Portal —
/// см. docs/e2e-register-participant.md.
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

        // ── Создание ЮЛ ─────────────────────────────────────────────
        Console.WriteLine($"[Seeder] ЮЛ {charterNumber}: создание ЮЛ...");
        Console.WriteLine($"[Seeder] ЮЛ {charterNumber}: {entity.Name} (ИНН {entity.Inn})...");
        await AdminConsoleHelper.CreateLegalEntityAsync(adminPage, entity.Name, entity.Inn);

        // ── Установка ОКОПФ (ООО = 12300) ──────────────────────────
        var selectedLeId = await adminPage.EvaluateAsync<string?>(
            @"() => {
                const url = new URL(window.location.href);
                return url.searchParams.get('le');
            }");
        if (!string.IsNullOrEmpty(selectedLeId) && Guid.TryParse(selectedLeId, out var leGuid))
        {
            Console.WriteLine($"[Seeder] ЮЛ {charterNumber}: установка ОКОПФ 12300 (ООО)...");
            await AdminConsoleHelper.SetOkopfAsync(adminPage, leGuid, "12300");
        }

        // ── Добавление LE_ADMIN в Admin Console ────────────────────
        Console.WriteLine($"[Seeder] ЮЛ {charterNumber}: добавление LE_ADMIN...");
        await AdminConsoleHelper.AddEmployeeAsync(
            adminPage,
            entity.AdminUser.LastName, entity.AdminUser.FirstName, entity.AdminUser.MiddleName,
            entity.AdminUser.Position, entity.AdminUser.Login,
            CharterTestDataFixed.RoleLeAdmin);

        Console.WriteLine($"[Seeder] ЮЛ {charterNumber}: сидирование завершено.");
    }
}
