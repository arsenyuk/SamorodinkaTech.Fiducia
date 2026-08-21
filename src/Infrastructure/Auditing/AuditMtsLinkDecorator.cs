using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models.MtsLink;

namespace SamorodinkaTech.Fiducia.Infrastructure.Auditing;

/// <summary>
/// Декоратор для IMtsLinkApiClient — логирует обращение к MTS Link API.
/// </summary>
public class AuditMtsLinkDecorator : IMtsLinkApiClient
{
    private readonly IMtsLinkApiClient _inner;
    private readonly ISecurityAuditService _auditService;
    private readonly IClientIpProvider _ipProvider;
    private readonly ILogger<AuditMtsLinkDecorator> _logger;

    public AuditMtsLinkDecorator(
        IMtsLinkApiClient inner,
        ISecurityAuditService auditService,
        IClientIpProvider ipProvider,
        ILogger<AuditMtsLinkDecorator> logger)
    {
        _inner = inner;
        _auditService = auditService;
        _ipProvider = ipProvider;
        _logger = logger;
    }

    public async Task<MtsLinkEventSession> CreateMeetingAsync(
        CreateMtsLinkMeetingRequest request,
        CancellationToken cancellationToken = default)
    {
        var clientIp = _ipProvider.GetClientIp();
        try
        {
            var result = await _inner.CreateMeetingAsync(request, cancellationToken);
            await _auditService.LogEventAsync("EXTERNAL:MtsLink:CreateMeeting", clientIp,
                $"Обращение к MTS Link API: CreateMeeting, sessionId={result.Id}, успех",
                entityName: "MtsLink");
            return result;
        }
        catch (Exception ex)
        {
            await _auditService.LogEventAsync("EXTERNAL:MtsLink:CreateMeeting", clientIp,
                $"Обращение к MTS Link API: CreateMeeting, ошибка={ex.Message}",
                entityName: "MtsLink");
            throw;
        }
    }

    public Task<MtsLinkEventSession?> GetEventSessionAsync(
        int eventSessionId,
        CancellationToken cancellationToken = default)
        => _inner.GetEventSessionAsync(eventSessionId, cancellationToken);

    public Task<bool> DeleteEventSessionAsync(
        int eventSessionId,
        bool sendEmail = false,
        CancellationToken cancellationToken = default)
        => _inner.DeleteEventSessionAsync(eventSessionId, sendEmail, cancellationToken);

    public Task StartEventSessionAsync(
        int eventSessionId,
        CancellationToken cancellationToken = default)
        => _inner.StartEventSessionAsync(eventSessionId, cancellationToken);

    public Task StopEventSessionAsync(
        int eventSessionId,
        CancellationToken cancellationToken = default)
        => _inner.StopEventSessionAsync(eventSessionId, cancellationToken);

    public Task<MtsLinkParticipation> RegisterParticipantAsync(
        int eventSessionId,
        RegisterMtsLinkParticipantRequest request,
        CancellationToken cancellationToken = default)
        => _inner.RegisterParticipantAsync(eventSessionId, request, cancellationToken);

    public Task<IReadOnlyList<MtsLinkParticipation>> GetParticipationsAsync(
        int eventSessionId,
        CancellationToken cancellationToken = default)
        => _inner.GetParticipationsAsync(eventSessionId, cancellationToken);
}
