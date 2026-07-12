using FluentAssertions;
using SamorodinkaTech.Fiducia.Domain.Models.TrueConf;
using SamorodinkaTech.Fiducia.Tests.Unit.Mocks;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Services;

/// <summary>
/// Unit-тесты TrueConf API клиента с использованием MockTrueConfApiClient.
/// Проверяют контракт ITrueConfApiClient: создание, чтение, удаление конференций,
/// авторизацию и обработку сбоев.
/// </summary>
public class TrueConfApiClientTests
{
    private readonly MockTrueConfApiClient _client = new();

    /// <summary>
    /// Получение токена возвращает непустой access-токен с префиксом mock.
    /// </summary>
    [Fact]
    public async Task GetTokenAsync_ShouldReturnToken()
    {
        var result = await _client.GetTokenAsync("test-id", "test-secret");

        result.AccessToken.Should().NotBeNullOrEmpty();
        result.AccessToken.Should().StartWith("mock-token-");
        _client.LastTokenIssued.Should().Be(result.AccessToken);
    }

    /// <summary>
    /// Создание конференции возвращает объект с корректными данными и ссылкой.
    /// </summary>
    [Fact]
    public async Task CreateConferenceAsync_ShouldReturnConferenceWithCorrectData()
    {
        var request = new CreateTrueConfConferenceRequest
        {
            DisplayName = "Заседание СД №42",
            StartTime = 1715942400,
            Duration = 3600,
            Tag = "board-meeting"
        };

        var result = await _client.CreateConferenceAsync(request);

        result.Should().NotBeNull();
        result.Id.Should().Be("1");
        result.DisplayName.Should().Be("Заседание СД №42");
        result.State.Should().Be("active");
        result.JoinLink.Should().NotBeNullOrEmpty();
        result.Schedule.Should().NotBeNull();
        result.Schedule!.Type.Should().Be(1);
        result.Schedule.StartTime.Should().Be(1715942400);
        result.Schedule.Duration.Should().Be(3600);
    }

    /// <summary>
    /// Каждое создание конференции получает уникальный ID.
    /// </summary>
    [Fact]
    public async Task CreateConferenceAsync_ShouldAssignUniqueIds()
    {
        var request = new CreateTrueConfConferenceRequest
        {
            DisplayName = "Конференция",
            StartTime = 0,
            Duration = 60
        };

        var conf1 = await _client.CreateConferenceAsync(request);
        var conf2 = await _client.CreateConferenceAsync(request);

        conf1.Id.Should().Be("1");
        conf2.Id.Should().Be("2");
        conf1.Id.Should().NotBe(conf2.Id);
    }

    /// <summary>
    /// Получение существующей конференции по ID возвращает объект с сохранёнными данными.
    /// </summary>
    [Fact]
    public async Task GetConferenceAsync_ExistingConference_ShouldReturnIt()
    {
        var created = await _client.CreateConferenceAsync(new CreateTrueConfConferenceRequest
        {
            DisplayName = "Тестовая конференция",
            StartTime = 0,
            Duration = 60
        });

        var result = await _client.GetConferenceAsync(created.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(created.Id);
        result.DisplayName.Should().Be("Тестовая конференция");
    }

    /// <summary>
    /// Получение несуществующей конференции по ID возвращает null.
    /// </summary>
    [Fact]
    public async Task GetConferenceAsync_NonExistingConference_ShouldReturnNull()
    {
        var result = await _client.GetConferenceAsync("999");

        result.Should().BeNull();
    }

    /// <summary>
    /// Удаление существующей конференции возвращает true, после чего конференция не находится.
    /// </summary>
    [Fact]
    public async Task DeleteConferenceAsync_ExistingConference_ShouldReturnTrue()
    {
        var created = await _client.CreateConferenceAsync(new CreateTrueConfConferenceRequest
        {
            DisplayName = "К удалению",
            StartTime = 0,
            Duration = 60
        });

        var deleted = await _client.DeleteConferenceAsync(created.Id);

        deleted.Should().BeTrue();
        var afterDelete = await _client.GetConferenceAsync(created.Id);
        afterDelete.Should().BeNull();
    }

    /// <summary>
    /// Удаление несуществующей конференции возвращает false.
    /// </summary>
    [Fact]
    public async Task DeleteConferenceAsync_NonExistingConference_ShouldReturnFalse()
    {
        var deleted = await _client.DeleteConferenceAsync("nonexistent");

        deleted.Should().BeFalse();
    }

    /// <summary>
    /// GetStoppedConferencesAsync возвращает только конференции со статусом stopped.
    /// </summary>
    [Fact]
    public async Task GetStoppedConferencesAsync_ShouldReturnOnlyStopped()
    {
        var active = await _client.CreateConferenceAsync(new CreateTrueConfConferenceRequest
        {
            DisplayName = "Активная",
            StartTime = 0,
            Duration = 60
        });

        _client.AddStoppedConference(new TrueConfConference
        {
            Id = "stopped-1",
            DisplayName = "Завершённая",
            State = "stopped"
        });

        var stopped = await _client.GetStoppedConferencesAsync();

        stopped.Should().HaveCount(1);
        stopped[0].Id.Should().Be("stopped-1");
    }

    /// <summary>
    /// GetUsersAsync возвращает всех добавленных пользователей.
    /// </summary>
    [Fact]
    public async Task GetUsersAsync_ShouldReturnAddedUsers()
    {
        _client.AddUser(new TrueConfUser
        {
            LoginName = "ivanov",
            DisplayName = "Иванов И.И.",
            Email = "ivanov@company.ru"
        });
        _client.AddUser(new TrueConfUser
        {
            LoginName = "petrov",
            DisplayName = "Петров П.П.",
            Email = "petrov@company.ru"
        });

        var users = await _client.GetUsersAsync();

        users.Should().HaveCount(2);
        users.Should().Contain(u => u.LoginName == "ivanov");
        users.Should().Contain(u => u.LoginName == "petrov");
    }

    /// <summary>
    /// При включённой симуляции сбоя все операции выбрасывают HttpRequestException.
    /// </summary>
    [Fact]
    public async Task SimulateFailure_ShouldThrowOnAllOperations()
    {
        _client.SimulateFailure = true;

        var tokenAct = () => _client.GetTokenAsync("id", "secret");
        await tokenAct.Should().ThrowAsync<HttpRequestException>();

        var createAct = () => _client.CreateConferenceAsync(new CreateTrueConfConferenceRequest
        {
            DisplayName = "x",
            StartTime = 0,
            Duration = 1
        });
        await createAct.Should().ThrowAsync<HttpRequestException>();

        var getAct = () => _client.GetConferenceAsync("1");
        await getAct.Should().ThrowAsync<HttpRequestException>();

        var deleteAct = () => _client.DeleteConferenceAsync("1");
        await deleteAct.Should().ThrowAsync<HttpRequestException>();

        var listStoppedAct = () => _client.GetStoppedConferencesAsync();
        await listStoppedAct.Should().ThrowAsync<HttpRequestException>();

        var usersAct = () => _client.GetUsersAsync();
        await usersAct.Should().ThrowAsync<HttpRequestException>();
    }

    /// <summary>
    /// Mock-клиент сохраняет данные между вызовами: три конференции доступны по своим ID.
    /// </summary>
    [Fact]
    public async Task Mock_ShouldPreserveDataAcrossCalls()
    {
        await _client.CreateConferenceAsync(new CreateTrueConfConferenceRequest
        {
            DisplayName = "Первая",
            StartTime = 100,
            Duration = 60
        });
        await _client.CreateConferenceAsync(new CreateTrueConfConferenceRequest
        {
            DisplayName = "Вторая",
            StartTime = 200,
            Duration = 60
        });
        await _client.CreateConferenceAsync(new CreateTrueConfConferenceRequest
        {
            DisplayName = "Третья",
            StartTime = 300,
            Duration = 60
        });

        var conf1 = await _client.GetConferenceAsync("1");
        var conf2 = await _client.GetConferenceAsync("2");
        var conf3 = await _client.GetConferenceAsync("3");

        conf1.Should().NotBeNull();
        conf1!.DisplayName.Should().Be("Первая");
        conf2.Should().NotBeNull();
        conf2!.DisplayName.Should().Be("Вторая");
        conf3.Should().NotBeNull();
        conf3!.DisplayName.Should().Be("Третья");
    }
}
