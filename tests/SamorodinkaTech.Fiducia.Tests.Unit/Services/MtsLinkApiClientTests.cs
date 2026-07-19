using FluentAssertions;
using SamorodinkaTech.Fiducia.Domain.Models.MtsLink;
using SamorodinkaTech.Fiducia.Tests.Unit.Mocks;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Services;

/// <summary>
/// Unit-тесты клиента API МТС Линк с использованием MockMtsLinkApiClient.
/// Проверяют контракт IMtsLinkApiClient: создание встреч (двухшаговое),
/// регистрацию участников, управление жизненным циклом и обработку сбоев.
/// </summary>
public class MtsLinkApiClientTests
{
    private readonly MockMtsLinkApiClient _client = new();

    /// <summary>
    /// Создание встречи возвращает сессию с корректными данными и ссылкой.
    /// </summary>
    [Fact]
    public async Task CreateMeetingAsync_ShouldReturnSessionWithCorrectData()
    {
        var request = new CreateMtsLinkMeetingRequest
        {
            Name = "Заседание СД №42",
            StartsAtTimestamp = "2025-06-09T20:15:00+03:00",
            Type = "meeting",
            Duration = "PT1H30M0S",
            Description = "Плановое заседание совета директоров",
            Tags = new[] { "board-meeting" }
        };

        var result = await _client.CreateMeetingAsync(request);

        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("Заседание СД №42");
        result.Type.Should().Be("meeting");
        result.Status.Should().Be("ACTIVE");
        result.Link.Should().NotBeNullOrEmpty();
        result.Link.Should().Contain("my.mts-link.ru");
    }

    /// <summary>
    /// Каждое создание встречи получает уникальный ID и уникальную ссылку.
    /// </summary>
    [Fact]
    public async Task CreateMeetingAsync_ShouldAssignUniqueIds()
    {
        var request = new CreateMtsLinkMeetingRequest
        {
            Name = "Тестовая встреча",
            StartsAtTimestamp = "2025-01-01T10:00:00+03:00",
            Type = "meeting"
        };

        var m1 = await _client.CreateMeetingAsync(request);
        var m2 = await _client.CreateMeetingAsync(request);

        m1.Id.Should().Be(1);
        m2.Id.Should().Be(2);
        m1.Link.Should().NotBe(m2.Link);
    }

    /// <summary>
    /// Получение существующей сессии по ID возвращает объект с сохранёнными данными.
    /// </summary>
    [Fact]
    public async Task GetEventSessionAsync_Existing_ShouldReturnIt()
    {
        var created = await _client.CreateMeetingAsync(new CreateMtsLinkMeetingRequest
        {
            Name = "Тестовая встреча",
            StartsAtTimestamp = "2025-01-01T10:00:00+03:00",
            Type = "meeting"
        });

        var result = await _client.GetEventSessionAsync(created.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(created.Id);
        result.Name.Should().Be("Тестовая встреча");
    }

    /// <summary>
    /// Получение несуществующей сессии по ID возвращает null.
    /// </summary>
    [Fact]
    public async Task GetEventSessionAsync_NonExisting_ShouldReturnNull()
    {
        var result = await _client.GetEventSessionAsync(999);

        result.Should().BeNull();
    }

    /// <summary>
    /// Удаление существующей сессии возвращает true, после чего сессия больше не находится.
    /// </summary>
    [Fact]
    public async Task DeleteEventSessionAsync_Existing_ShouldReturnTrue()
    {
        var created = await _client.CreateMeetingAsync(new CreateMtsLinkMeetingRequest
        {
            Name = "К удалению",
            StartsAtTimestamp = "2025-01-01T10:00:00+03:00",
            Type = "meeting"
        });

        var deleted = await _client.DeleteEventSessionAsync(created.Id);

        deleted.Should().BeTrue();
        var after = await _client.GetEventSessionAsync(created.Id);
        after.Should().BeNull();
    }

    /// <summary>
    /// Удаление несуществующей сессии возвращает false.
    /// </summary>
    [Fact]
    public async Task DeleteEventSessionAsync_NonExisting_ShouldReturnFalse()
    {
        var deleted = await _client.DeleteEventSessionAsync(999);

        deleted.Should().BeFalse();
    }

    /// <summary>
    /// Остановка и повторный запуск сессии корректно меняют статус ACTIVE ↔ STOP.
    /// </summary>
    [Fact]
    public async Task StartStopEventSession_ShouldChangeStatus()
    {
        var created = await _client.CreateMeetingAsync(new CreateMtsLinkMeetingRequest
        {
            Name = "Встреча",
            StartsAtTimestamp = "2025-01-01T10:00:00+03:00",
            Type = "meeting"
        });

        await _client.StopEventSessionAsync(created.Id);

        var stopped = await _client.GetEventSessionAsync(created.Id);
        stopped!.Status.Should().Be("STOP");

        await _client.StartEventSessionAsync(created.Id);

        var restarted = await _client.GetEventSessionAsync(created.Id);
        restarted!.Status.Should().Be("ACTIVE");
    }

    /// <summary>
    /// Регистрация участника возвращает объект с participationId, ссылкой и contactId.
    /// </summary>
    [Fact]
    public async Task RegisterParticipantAsync_ShouldReturnParticipationWithLink()
    {
        var meeting = await _client.CreateMeetingAsync(new CreateMtsLinkMeetingRequest
        {
            Name = "Заседание СД",
            StartsAtTimestamp = "2025-01-01T10:00:00+03:00",
            Type = "meeting"
        });

        var result = await _client.RegisterParticipantAsync(meeting.Id, new RegisterMtsLinkParticipantRequest
        {
            Email = "ivanov@company.ru",
            Name = "Иван",
            SecondName = "Иванов",
            Role = "LECTURER",
            SendEmail = false
        });

        result.Should().NotBeNull();
        result.ParticipationId.Should().Be(1);
        result.Link.Should().NotBeNullOrEmpty();
        result.ContactId.Should().NotBeNull();
    }

    /// <summary>
    /// Регистрация участника в несуществующей сессии выбрасывает KeyNotFoundException.
    /// </summary>
    [Fact]
    public async Task RegisterParticipantAsync_NonExistingSession_ShouldThrow()
    {
        var act = () => _client.RegisterParticipantAsync(999, new RegisterMtsLinkParticipantRequest
        {
            Email = "test@test.ru"
        });

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    /// <summary>
    /// Регистрация нескольких участников: каждый получает уникальный ID и ссылку.
    /// </summary>
    [Fact]
    public async Task RegisterMultipleParticipants_ShouldGetUniqueIds()
    {
        var meeting = await _client.CreateMeetingAsync(new CreateMtsLinkMeetingRequest
        {
            Name = "Заседание СД",
            StartsAtTimestamp = "2025-01-01T10:00:00+03:00",
            Type = "meeting"
        });

        var p1 = await _client.RegisterParticipantAsync(meeting.Id,
            new RegisterMtsLinkParticipantRequest { Email = "dir1@company.ru" });
        var p2 = await _client.RegisterParticipantAsync(meeting.Id,
            new RegisterMtsLinkParticipantRequest { Email = "dir2@company.ru" });
        var p3 = await _client.RegisterParticipantAsync(meeting.Id,
            new RegisterMtsLinkParticipantRequest { Email = "dir3@company.ru" });

        p1.ParticipationId.Should().Be(1);
        p2.ParticipationId.Should().Be(2);
        p3.ParticipationId.Should().Be(3);
        p1.Link.Should().NotBe(p2.Link);
    }

    /// <summary>
    /// При включённой симуляции сбоя все операции выбрасывают HttpRequestException.
    /// </summary>
    [Fact]
    public async Task SimulateFailure_ShouldThrowOnAllOperations()
    {
        _client.SimulateFailure = true;

        var createAct = () => _client.CreateMeetingAsync(new CreateMtsLinkMeetingRequest
        {
            Name = "x",
            StartsAtTimestamp = "2025-01-01T00:00:00+03:00",
            Type = "meeting"
        });
        await createAct.Should().ThrowAsync<HttpRequestException>();

        var getAct = () => _client.GetEventSessionAsync(1);
        await getAct.Should().ThrowAsync<HttpRequestException>();

        var deleteAct = () => _client.DeleteEventSessionAsync(1);
        await deleteAct.Should().ThrowAsync<HttpRequestException>();

        var startAct = () => _client.StartEventSessionAsync(1);
        await startAct.Should().ThrowAsync<HttpRequestException>();

        var stopAct = () => _client.StopEventSessionAsync(1);
        await stopAct.Should().ThrowAsync<HttpRequestException>();

        var registerAct = () => _client.RegisterParticipantAsync(1,
            new RegisterMtsLinkParticipantRequest { Email = "x@x.ru" });
        await registerAct.Should().ThrowAsync<HttpRequestException>();
    }
}
