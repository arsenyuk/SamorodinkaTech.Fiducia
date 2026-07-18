using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models.MtsLink;

namespace SamorodinkaTech.Fiducia.Tests.Unit.Mocks;

/// <summary>
/// Mock-реализация IMtsLinkApiClient для unit-тестирования.
/// Хранит данные в оперативной памяти и имитирует поведение API МТС Линк.
/// </summary>
public class MockMtsLinkApiClient : IMtsLinkApiClient
{
    private readonly Dictionary<int, MtsLinkEventSession> _sessions = new();
    private readonly Dictionary<int, List<MtsLinkParticipation>> _participations = new();
    private int _nextId = 1;

    /// <summary>Если true, все методы выбрасывают исключение (имитация сбоя).</summary>
    public bool SimulateFailure { get; set; }

    /// <inheritdoc />
    public Task<MtsLinkEventSession> CreateMeetingAsync(
        CreateMtsLinkMeetingRequest request,
        CancellationToken cancellationToken = default)
    {
        ThrowIfFailure();

        var id = _nextId++;
        var session = new MtsLinkEventSession
        {
            Id = id,
            Name = request.Name,
            Status = "ACTIVE",
            StartsAt = request.StartsAtTimestamp,
            Link = $"https://my.mts-link.ru/j/{86000000 + id}/{43000000 + id}",
            Type = request.Type
        };

        _sessions[id] = session;
        _participations[id] = new List<MtsLinkParticipation>();

        return Task.FromResult(session);
    }

    /// <inheritdoc />
    public Task<MtsLinkEventSession?> GetEventSessionAsync(
        int eventSessionId,
        CancellationToken cancellationToken = default)
    {
        ThrowIfFailure();

        _sessions.TryGetValue(eventSessionId, out var session);
        return Task.FromResult(session);
    }

    /// <inheritdoc />
    public Task<bool> DeleteEventSessionAsync(
        int eventSessionId,
        bool sendEmail = false,
        CancellationToken cancellationToken = default)
    {
        ThrowIfFailure();

        var removed = _sessions.Remove(eventSessionId);
        _participations.Remove(eventSessionId);
        return Task.FromResult(removed);
    }

    /// <inheritdoc />
    public Task StartEventSessionAsync(
        int eventSessionId,
        CancellationToken cancellationToken = default)
    {
        ThrowIfFailure();

        if (_sessions.TryGetValue(eventSessionId, out var session))
        {
            _sessions[eventSessionId] = session with { Status = "ACTIVE" };
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopEventSessionAsync(
        int eventSessionId,
        CancellationToken cancellationToken = default)
    {
        ThrowIfFailure();

        if (_sessions.TryGetValue(eventSessionId, out var session))
        {
            _sessions[eventSessionId] = session with { Status = "STOP" };
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<MtsLinkParticipation> RegisterParticipantAsync(
        int eventSessionId,
        RegisterMtsLinkParticipantRequest request,
        CancellationToken cancellationToken = default)
    {
        ThrowIfFailure();

        if (!_participations.TryGetValue(eventSessionId, out var list))
            throw new KeyNotFoundException($"EventSession {eventSessionId} не найден");

        var participation = new MtsLinkParticipation
        {
            ParticipationId = list.Count + 1,
            Link = $"https://my.mts-link.ru/j/{86000000 + eventSessionId}/{43000000 + eventSessionId}/{Guid.NewGuid():N}",
            ContactId = list.Count + 1000
        };

        list.Add(participation);
        return Task.FromResult(participation);
    }

    /// <summary>
    /// Добавляет существующее мероприятие в mock-хранилище (для преднастройки тестов).
    /// </summary>
    public void AddEventSession(MtsLinkEventSession session)
    {
        _sessions[session.Id] = session;
        _participations[session.Id] = new List<MtsLinkParticipation>();
    }

    private void ThrowIfFailure()
    {
        if (SimulateFailure)
            throw new HttpRequestException("Simulated MTS Link API failure");
    }
}
