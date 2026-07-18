using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models.TrueConf;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Mocks;

/// <summary>
/// Mock-реализация ITrueConfApiClient для unit-тестирования.
/// Хранит данные в оперативной памяти и имитирует поведение TrueConf Server API.
/// </summary>
public class MockTrueConfApiClient : ITrueConfApiClient
{
    private readonly Dictionary<string, TrueConfConference> _conferences = new();
    private readonly List<TrueConfUser> _users = new();
    private int _nextId = 1;

    /// <summary>Задержка ответа в миллисекундах для имитации сети (по умолчанию 0).</summary>
    public int SimulatedDelayMs { get; set; }

    /// <summary>Если true, все методы выбрасывают исключение (имитация сбоя сервера).</summary>
    public bool SimulateFailure { get; set; }

    /// <summary>Последний выданный токен.</summary>
    public string? LastTokenIssued { get; private set; }

    /// <inheritdoc />
    public Task<TrueConfTokenResponse> GetTokenAsync(
        string clientId,
        string clientSecret,
        CancellationToken cancellationToken = default)
    {
        ThrowIfFailure();

        LastTokenIssued = $"mock-token-{Guid.NewGuid():N}";
        return Task.FromResult(new TrueConfTokenResponse
        {
            AccessToken = LastTokenIssued
        });
    }

    /// <inheritdoc />
    public Task<TrueConfConference> CreateConferenceAsync(
        CreateTrueConfConferenceRequest request,
        CancellationToken cancellationToken = default)
    {
        ThrowIfFailure();

        var id = (_nextId++).ToString();
        var conference = new TrueConfConference
        {
            Id = id,
            DisplayName = request.DisplayName,
            State = "active",
            Schedule = new TrueConfSchedule
            {
                Type = 1,
                StartTime = request.StartTime,
                Duration = request.Duration
            },
            JoinLink = $"https://video.company.ru/c/{id}"
        };

        _conferences[id] = conference;
        return Task.FromResult(conference);
    }

    /// <inheritdoc />
    public Task<TrueConfConference?> GetConferenceAsync(
        string conferenceId,
        CancellationToken cancellationToken = default)
    {
        ThrowIfFailure();

        _conferences.TryGetValue(conferenceId, out var conference);
        return Task.FromResult(conference);
    }

    /// <inheritdoc />
    public Task<bool> DeleteConferenceAsync(
        string conferenceId,
        CancellationToken cancellationToken = default)
    {
        ThrowIfFailure();

        return Task.FromResult(_conferences.Remove(conferenceId));
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<TrueConfConference>> GetStoppedConferencesAsync(
        string? tag = null,
        CancellationToken cancellationToken = default)
    {
        ThrowIfFailure();

        var result = _conferences.Values
            .Where(c => c.State == "stopped")
            .AsEnumerable();

        if (!string.IsNullOrEmpty(tag))
            result = result.Where(c => c.Schedule != null);

        return Task.FromResult<IReadOnlyList<TrueConfConference>>(result.ToList());
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<TrueConfUser>> GetUsersAsync(
        CancellationToken cancellationToken = default)
    {
        ThrowIfFailure();

        return Task.FromResult<IReadOnlyList<TrueConfUser>>(_users.ToList());
    }

    /// <summary>
    /// Добавляет пользователя в mock-хранилище (для настройки тестовых данных).
    /// </summary>
    public void AddUser(TrueConfUser user)
    {
        _users.Add(user);
    }

    /// <summary>
    /// Добавляет завершённую конференцию в mock-хранилище (для настройки тестовых данных).
    /// </summary>
    public void AddStoppedConference(TrueConfConference conference)
    {
        _conferences[conference.Id] = conference;
    }

    private void ThrowIfFailure()
    {
        if (SimulateFailure)
            throw new HttpRequestException("Simulated TrueConf Server failure");
    }
}
