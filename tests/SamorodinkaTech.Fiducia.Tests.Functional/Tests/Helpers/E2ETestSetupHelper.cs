using FluentAssertions;
using Microsoft.Playwright;
using System.Text.Json;

namespace SamorodinkaTech.Fiducia.Tests.Functional.Helpers;

/// <summary>
/// Модель данных лица из единого файла тестовых данных (tools/test-data.json).
/// </summary>
public class TestDataPerson
{
    public string Login { get; set; } = "";
    public string LastName { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string MiddleName { get; set; } = "";
    public string Inn { get; set; } = "";
    public string Snils { get; set; } = "";
    public string DulType { get; set; } = "";
    public string DulSeries { get; set; } = "";
    public string DulNumber { get; set; } = "";
    public string LdapUid { get; set; } = "";
    public string LdapDn { get; set; } = "";
    public string FullName { get; set; } = "";
    public string? Position { get; set; }
    public decimal? SharePercent { get; set; }
    public bool IsDirector { get; set; }
    public string[] Roles { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Корневая модель unified файла тестовых данных.
/// </summary>
public class TestDataRoot
{
    public TestDataPerson[] Persons { get; set; } = Array.Empty<TestDataPerson>();
}

/// <summary>
/// Хелпер для загрузки единых тестовых данных из tools/test-data.json.
/// </summary>
public static class TestDataLoader
{
    private static TestDataRoot? _cache;

    /// <summary>
    /// Загрузить единые данные из tools/test-data.json (с кешированием).
    /// </summary>
    public static TestDataRoot Load()
    {
        if (_cache is not null)
            return _cache;

        var jsonPath = Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..", "tools", "test-data.json");

        if (!File.Exists(jsonPath))
        {
            // Fallback: пробуем относительно текущей директории
            jsonPath = Path.Combine("tools", "test-data.json");
        }

        var json = File.ReadAllText(jsonPath);
        _cache = JsonSerializer.Deserialize<TestDataRoot>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new TestDataRoot();

        return _cache;
    }

    /// <summary>
    /// Найти лицо по логину.
    /// </summary>
    public static TestDataPerson FindByLogin(string login)
    {
        var data = Load();
        return data.Persons.FirstOrDefault(p => p.Login == login)
            ?? throw new InvalidOperationException($"Лицо с логином '{login}' не найдено в test-data.json");
    }

    /// <summary>
    /// Найти лицо по полному ФИО.
    /// </summary>
    public static TestDataPerson FindByFullName(string fullName)
    {
        var data = Load();
        return data.Persons.FirstOrDefault(p => p.FullName == fullName)
            ?? throw new InvalidOperationException($"Лицо с ФИО '{fullName}' не найдено в test-data.json");
    }
}

/// <summary>
/// Хелпер для стандартной подготовки E2E-тестов.
/// Использует единый файл тестовых данных (tools/test-data.json).
/// </summary>
public static class E2ETestSetupHelper
{
    /// <summary>
    /// Полная подготовка участника: создание User в Admin Console + BoardParticipant + ЕДИН binding.
    /// Данные берутся из единого файла тестовых данных.
    /// </summary>
    /// <returns>participantId (BoardParticipant.Id)</returns>
    public static async Task<Guid> SetupParticipantAsync(
        IPage adminPage,
        IPage boardPage,
        CharterTestDataFixed.PersonData participant,
        int entityIndex)
    {
        var entity = CharterTestDataFixed.LegalEntities[entityIndex - 1];
        var persons = CharterTestDataFixed.PersonsByEntity[entityIndex];
        var gdLogin = persons.Gd?.Login ?? persons.Participants[0].Login;

        // Загружаем единые данные для участника
        var testData = TestDataLoader.FindByLogin(participant.Login);

        // 1. Создание User для участника в Admin Console
        Console.WriteLine($"[Setup] Создание User для участника {participant.Login} в Admin Console...");
        await AuthHelper.LoginAsAdminAsync(adminPage, CharterTestDataFixed.SysAdminLogin);
        await AdminConsoleHelper.NavigateToLegalEntityAsync(adminPage, entity.Name);
        await AdminConsoleHelper.AddEmployeeAsync(
            adminPage,
            testData.LastName, testData.FirstName, testData.MiddleName,
            "Участник", testData.Login,
            CharterTestDataFixed.RoleLeAdmin);
        Console.WriteLine($"[Setup] User {testData.Login} создан.");

        // 2. Регистрация BoardParticipant через Board Portal
        await AuthHelper.LoginAsBoardUserAsync(boardPage, gdLogin);
        boardPage.Url.Should().Contain("/main");

        // Ищем существующий EcosystemParticipant по ФИО
        var ecoId = await boardPage.EvaluateAsync<Guid?>(
            $@"async () => {{
                const response = await fetch('/api/participants/eco-search?name={Uri.EscapeDataString(testData.FullName)}', {{
                    credentials: 'same-origin'
                }});
                if (!response.ok) return null;
                const data = await response.json();
                if (data && data.length > 0 && data[0].id) return data[0].id;
                return null;
            }}");

        // Используем единые данные ДУЛ из JSON
        // Если доля < 100%, добавляем сведения об оплате (обязательное поле API)
        var paymentInfo = participant.SharePercent < 100 ? "Оплачено полностью" : null;
        var participantId = await BoardPortalHelper.AddParticipantWithPersonalDataAsync(
            boardPage,
            fullName: testData.FullName,
            dulTypeCode: testData.DulType,
            passportSeries: testData.DulSeries,
            passportNumber: testData.DulNumber,
            personInn: testData.Inn,
            participantType: "FL",
            sharePercent: participant.SharePercent,
            shareAmount: participant.SharePercent * 100m,
            ecosystemParticipantId: ecoId,
            paymentInfo: paymentInfo);

        participantId.Should().NotBeEmpty("участник должен быть создан");
        Console.WriteLine($"[Setup] BoardParticipant создан: {participantId}");

        // 3. Ожидание ЕДИН binding
        Console.WriteLine("[Setup] Ожидание ЕДИН binding...");
        await EdinTestHelper.WaitForEdinBindingAsync(boardPage, participantId, timeoutSeconds: 10);

        var mpiMasterId = await EdinTestHelper.GetParticipantMpiMasterIdAsync(boardPage, participantId);
        mpiMasterId.Should().NotBeNull("ЕДИН должен привязать MasterId");
        Console.WriteLine($"[Setup] ЕДИН binding завершён: {mpiMasterId}");

        return participantId;
    }

    /// <summary>
    /// Полная подготовка ГД: регистрация как BoardParticipant + назначение через LegalEntities.razor.
    /// Присваивает роль CEO. Данные берутся из единого файла тестовых данных.
    /// </summary>
    public static async Task SetupGdAsync(
        IPage boardPage,
        CharterTestDataFixed.PersonData gd,
        int entityIndex)
    {
        // Загружаем единые данные для ГД
        var testData = TestDataLoader.FindByLogin(gd.Login);

        // 1. Регистрация ГД как BoardParticipant
        Console.WriteLine($"[Setup] Регистрация ГД {testData.Login} как BoardParticipant...");
        await AuthHelper.LoginAsBoardUserAsync(boardPage, testData.Login);

        // Используем единые данные ДУЛ из JSON
        var gdParticipantId = await BoardPortalHelper.AddParticipantWithPersonalDataAsync(
            boardPage,
            fullName: testData.FullName,
            dulTypeCode: testData.DulType,
            passportSeries: testData.DulSeries,
            passportNumber: testData.DulNumber,
            personInn: testData.Inn,
            participantType: "FL",
            sharePercent: 100m,
            shareAmount: 100000m,
            ecosystemParticipantId: null);

        gdParticipantId.Should().NotBeEmpty("ГД должен быть создан как BoardParticipant");
        Console.WriteLine($"[Setup] ГД зарегистрирован как BoardParticipant: {gdParticipantId}");

        // 2. Назначение ГД через LegalEntities.razor (присваивает роль CEO)
        Console.WriteLine("[Setup] Назначение ГД через LegalEntities.razor...");
        await BoardPortalHelper.NavigateToAsync(boardPage, "legal-entities");
        await BoardPortalHelper.ClickGeneralDirectorTabAsync(boardPage);
        await BoardPortalHelper.SelectGeneralDirectorAsync(boardPage, testData.FullName);
        await BoardPortalHelper.SaveAndVerifyAsync(boardPage);

        // Ожидание завершения ЕДИН-интеграции (асинхронный Task.Run в LegalEntities.razor)
        Console.WriteLine("[Setup] Ожидание ЕДИН-интеграции для ГД...");
        await boardPage.WaitForTimeoutAsync(GlobalFixture.TestOptions.TimeoutMs * 2);
        Console.WriteLine("[Setup] ГД назначен, роль CEO назначена.");
    }

    /// <summary>
    /// Полная подготовка для теста, где ГД = LE_ADMIN: participant + GD setup.
    /// </summary>
    /// <returns>(participantId, gdLogin)</returns>
    public static async Task<(Guid participantId, string gdLogin)> SetupFullTestAsync(
        IPage adminPage,
        IPage boardPage,
        int entityIndex)
    {
        var persons = CharterTestDataFixed.PersonsByEntity[entityIndex];
        var gd = persons.Gd!;
        var participant = persons.Participants[0];
        var gdLogin = gd.Login;

        // 1. Подготовка участника
        var participantId = await SetupParticipantAsync(adminPage, boardPage, participant, entityIndex);

        // 2. Подготовка ГД (если ГД ≠ участник)
        if (gdLogin != participant.Login)
        {
            await SetupGdAsync(boardPage, gd, entityIndex);
        }

        return (participantId, gdLogin);
    }
}
