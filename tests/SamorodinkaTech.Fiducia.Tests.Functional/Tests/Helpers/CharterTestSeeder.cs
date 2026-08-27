using FluentAssertions;
using Microsoft.Playwright;

namespace SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

/// <summary>
/// Сидер базы данных для E2E-тестов уставов.
/// Выполняет сброс БД и создание всех ЮЛ + LDAP-пользователей ОДИН раз перед прогоном тестов.
/// Использует идемпотентную инициализацию: повторный вызов ничего не делает.
/// При ошибке сидирования — все последующие тесты немедленно падают с сохранённым исключением.
/// </summary>
public static class CharterTestSeeder
{
    private static bool _seeded;
    private static Exception? _seedingException;
    private static readonly SemaphoreSlim Semaphore = new(1, 1);

    /// <summary>
    /// Убедиться, что БД засеяна данными для всех уставов.
    /// Вызывать в начале каждого E2E-теста — повторные вызовы ничего не делают.
    /// При ошибке сидирования — немедленно бросает сохранённое исключение.
    /// </summary>
    public static async Task<IPage?> EnsureSeededAsync(IPage adminPage, IPage ldapPage, Func<Task<IPage>>? createAdminPage = null)
    {
        if (_seedingException is not null)
        {
            throw new InvalidOperationException(
                $"[Seeder] Сидирование уже завершилось ошибкой. Все тесты пропускаются. " +
                $"Ошибка: {_seedingException.Message}", _seedingException);
        }

        if (_seeded) return adminPage;

        await Semaphore.WaitAsync();
        try
        {
            if (_seedingException is not null)
            {
                throw new InvalidOperationException(
                    $"[Seeder] Сидирование уже завершилось ошибкой. Все тесты пропускаются. " +
                    $"Ошибка: {_seedingException.Message}", _seedingException);
            }

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
        // Шаг 3: Создание всех ЮЛ + назначение ролей + создание пользователей
        // ═══════════════════════════════════════════════════════════════════
        foreach (var entity in CharterTestDataFixed.LegalEntities)
        {
            Console.WriteLine($"[Seeder] Создание ЮЛ: {entity.Name} (ИНН {entity.Inn})...");
            try
            {
                await AdminConsoleHelper.CreateLegalEntityAsync(adminPage, entity.Name, entity.Inn);
                Console.WriteLine($"[Seeder] ЮЛ создано: {entity.Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Seeder] ОШИБКА создания ЮЛ {entity.Name} (ИНН {entity.Inn}): {ex.Message}");
                throw;
            }

            var persons = CharterTestDataFixed.PersonsByEntity[entity.Number];

            // Создание пользователя + назначение ролей для ГД
            if (persons.Gd is not null)
            {
                Console.WriteLine($"[Seeder] Создание пользователя ГД: {persons.Gd.LastName} {persons.Gd.FirstName}...");
                try
                {
                    var email = $"{persons.Gd.Uid}@test.local";
                    var phone = $"+7900{entity.Number:D4}0001";
                    await AdminConsoleHelper.CreateUserAsync(
                        adminPage,
                        persons.Gd.LastName,
                        persons.Gd.FirstName,
                        persons.Gd.MiddleName,
                        email,
                        phone);
                    Console.WriteLine($"[Seeder] Пользователь создан: {persons.Gd.Uid}");

                    await AdminConsoleHelper.AssignRolesAsync(
                        adminPage,
                        persons.Gd.LastName,
                        persons.Gd.FirstName,
                        persons.Gd.MiddleName,
                        persons.Gd.Position,
                        persons.Gd.Uid,
                        [CharterTestDataFixed.RoleLeAdmin, CharterTestDataFixed.RoleCeo]);
                    Console.WriteLine($"[Seeder] Роли назначены: {persons.Gd.LastName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Seeder] ОШИБКА для ГД {persons.Gd.LastName}: {ex.Message}");
                    throw;
                }
            }
            else if (persons.Participants.Count > 0)
            {
                // Нет ГД — назначаем роли первому участнику
                var firstParticipant = persons.Participants[0];
                var nameParts = firstParticipant.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (nameParts.Length >= 3)
                {
                    var login = firstParticipant.FullName.ToLower().Replace(" ", ".");
                    Console.WriteLine($"[Seeder] Создание пользователя участника: {firstParticipant.FullName}...");
                    try
                    {
                        var email = $"{login}@test.local";
                        var phone = $"+7900{entity.Number:D4}0002";
                        await AdminConsoleHelper.CreateUserAsync(
                            adminPage,
                            nameParts[0],
                            nameParts[1],
                            nameParts[2],
                            email,
                            phone);
                        Console.WriteLine($"[Seeder] Пользователь создан: {login}");

                        await AdminConsoleHelper.AssignRolesAsync(
                            adminPage,
                            nameParts[0],
                            nameParts[1],
                            nameParts[2],
                            "Директор",
                            login,
                            [CharterTestDataFixed.RoleLeAdmin, CharterTestDataFixed.RoleCeo]);
                        Console.WriteLine($"[Seeder] Роли назначены: {firstParticipant.FullName}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Seeder] ОШИБКА для {firstParticipant.FullName}: {ex.Message}");
                        throw;
                    }
                }
            }
        }

        Console.WriteLine("[Seeder] Сидирование завершено.");
        return adminPage;
    }
}
