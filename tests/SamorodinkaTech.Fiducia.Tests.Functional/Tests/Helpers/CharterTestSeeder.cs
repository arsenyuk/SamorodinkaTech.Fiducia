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
    public static async Task<IPage?> EnsureSeededAsync(IPage adminPage, IPage ldapPage, Func<Task<IPage>>? createAdminPage = null)
    {
        if (_seeded) return adminPage;

        await Semaphore.WaitAsync();
        try
        {
            if (_seeded) return adminPage;

            var result = await SeedAsync(adminPage, ldapPage, createAdminPage);
            _seeded = true;
            return result;
        }
        finally
        {
            Semaphore.Release();
        }
    }

    /// <summary>
    /// Полная процедура сидирования: инфраструктура → сброс БД → LDAP → Admin Console → создание ЮЛ → роли → участники.
    /// Возвращает залогиненную страницу Admin Console.
    /// </summary>
    private static async Task<IPage> SeedAsync(IPage adminPage, IPage ldapPage, Func<Task<IPage>>? createAdminPage = null)
    {
        Console.WriteLine("[Seeder] Начало сидирования...");

        // ═══════════════════════════════════════════════════════════════════
        // Шаг 1: Логин SYS_ADMIN в Admin Console
        // ═══════════════════════════════════════════════════════════════════
        Console.WriteLine("[Seeder] Шаг 1: Логин SYS_ADMIN...");
        await AuthHelper.LoginAsAdminAsync(adminPage, CharterTestDataFixed.SysAdminLogin);
        adminPage.Url.Should().Contain("/main");
        Console.WriteLine("[Seeder] Шаг 1: Логин выполнен.");

        // ═══════════════════════════════════════════════════════════════════
        // Шаг 2: Переход в режим Пользователи
        // ═══════════════════════════════════════════════════════════════════
        await adminPage.GotoAsync(PortalUrls.GetUrl(Portal.AdminConsole, "/access-management"));
        await AuthHelper.WaitForBlazorReady(adminPage);
        await adminPage.WaitForTimeoutAsync(1000);

        // ═══════════════════════════════════════════════════════════════════
        // Шаг 3: Создание всех ЮЛ + назначение ролей
        // ═══════════════════════════════════════════════════════════════════
        foreach (var entity in CharterTestDataFixed.LegalEntities)
        {
            await AdminConsoleHelper.CreateLegalEntityAsync(adminPage, entity.Name, entity.Inn);

            var persons = CharterTestDataFixed.PersonsByEntity[entity.Number];

            if (persons.Gd is not null)
            {
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
                var firstParticipant = persons.Participants[0];
                var nameParts = firstParticipant.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (nameParts.Length >= 3)
                {
                    await AdminConsoleHelper.AssignRolesAsync(
                        adminPage,
                        nameParts[0],
                        nameParts[1],
                        nameParts[2],
                        "Директор",
                        firstParticipant.FullName.ToLower().Replace(" ", "."),
                        [CharterTestDataFixed.RoleLeAdmin, CharterTestDataFixed.RoleCeo]);
                }
            }
        }

        Console.WriteLine("[Seeder] Сидирование завершено.");
        return adminPage;
    }
}
