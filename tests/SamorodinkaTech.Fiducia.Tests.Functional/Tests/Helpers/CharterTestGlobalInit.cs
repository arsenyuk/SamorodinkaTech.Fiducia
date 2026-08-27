using Microsoft.Playwright;

namespace SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

/// <summary>
/// Глобальная инициализация для E2E-тестов уставов.
/// Выполняется ОДИН раз перед прогоном ВСЕХ тестов:
/// 1. Запуск инфраструктуры (порталы, LDAP)
/// 2. Сброс БД
/// 3. Удаление всех LDAP-пользователей
/// 4. Создание LDAP-пользователей для всех ЮЛ
/// </summary>
public static class CharterTestGlobalInit
{
    private static bool _initialized;
    private static readonly SemaphoreSlim Semaphore = new(1, 1);

    /// <summary>
    /// Выполнить глобальную инициализацию ОДИН раз перед прогоном всех тестов.
    /// </summary>
    public static async Task InitializeAsync(IPage adminPage, IPage ldapPage, Func<Task<IPage>>? createAdminPage = null)
    {
        if (_initialized) return;

        await Semaphore.WaitAsync();
        try
        {
            if (_initialized) return;

            Console.WriteLine("[GlobalInit] === Начало глобальной инициализации ===");

            // ═══════════════════════════════════════════════════════════════
            // Шаг 1: Запуск инфраструктуры
            // ═══════════════════════════════════════════════════════════════
            Console.WriteLine("[GlobalInit] Шаг 1: Проверка инфраструктуры...");
            await InfrastructureHelper.EnsureInfrastructureReadyAsync();

            // ═══════════════════════════════════════════════════════════════
            // Шаг 2: Сброс БД
            // ═══════════════════════════════════════════════════════════════
            Console.WriteLine("[GlobalInit] Шаг 2: Сброс БД...");
            await DbResetHelper.ResetAsync(includeDemo: false, timeout: TimeSpan.FromMinutes(3));
            Console.WriteLine("[GlobalInit] Шаг 2: БД сброшена.");

            // ═══════════════════════════════════════════════════════════════
            // Шаг 3: Удаление всех LDAP-пользователей
            // ═══════════════════════════════════════════════════════════════
            Console.WriteLine("[GlobalInit] Шаг 3: Удаление LDAP-пользователей...");
            await LdapHelper.DeleteAllTestUsersAsync();
            Console.WriteLine("[GlobalInit] Шаг 3: LDAP-пользователи удалены.");

            // ═══════════════════════════════════════════════════════════════
            // Шаг 4: Создание LDAP-пользователей для всех ЮЛ
            // ═══════════════════════════════════════════════════════════════
            Console.WriteLine("[GlobalInit] Шаг 4: Создание LDAP-пользователей...");
            var allPersons = GetAllUniquePersons();
            var personsList = allPersons.Where(p => !string.IsNullOrEmpty(p.Uid)).ToList();
            Console.WriteLine($"[GlobalInit] Шаг 4: Найдено {personsList.Count} уникальных лиц для создания.");

            foreach (var person in personsList)
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
            Console.WriteLine("[GlobalInit] Шаг 4: LDAP-пользователи созданы.");

            _initialized = true;
            Console.WriteLine("[GlobalInit] === Глобальная инициализация завершена ===");
        }
        finally
        {
            Semaphore.Release();
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
            // Администратор ЮЛ
            if (seen.Add(entity.AdminUser.Uid))
            {
                yield return entity.AdminUser;
            }

            if (!CharterTestDataFixed.PersonsByEntity.TryGetValue(entity.Number, out var persons))
                continue;

            if (persons.Gd is not null && seen.Add(persons.Gd.Uid))
            {
                yield return persons.Gd;
            }

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
