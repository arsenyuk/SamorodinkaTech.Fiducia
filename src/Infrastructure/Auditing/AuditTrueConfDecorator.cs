using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models.TrueConf;

namespace SamorodinkaTech.Fiducia.Infrastructure.Auditing;

/// <summary>
/// Декоратор для ITrueConfApiClient — логирует авторизацию (GetTokenAsync).
/// </summary>
public class AuditTrueConfDecorator : ITrueConfApiClient
{
    private readonly ITrueConfApiClient _inner;
    private readonly ISecurityAuditService _auditService;
    private readonly IClientIpProvider _ipProvider;
    private readonly ILogger<AuditTrueConfDecorator> _logger;

    public AuditTrueConfDecorator(
        ITrueConfApiClient inner,
        ISecurityAuditService auditService,
        IClientIpProvider ipProvider,
        ILogger<AuditTrueConfDecorator> logger)
    {
        _inner = inner;
        _auditService = auditService;
        _ipProvider = ipProvider;
        _logger = logger;
    }

    public async Task<TrueConfTokenResponse> GetTokenAsync(
        string clientId,
        string clientSecret,
        CancellationToken cancellationToken = default)
    {
        var clientIp = _ipProvider.GetClientIp();
        try
        {
            var result = await _inner.GetTokenAsync(clientId, clientSecret, cancellationToken);
            await _auditService.LogEventAsync("EXTERNAL:TrueConf:Auth", clientIp,
                $"Авторизация TrueConf: clientId={clientId}, результат=успех",
                entityName: "TrueConf");
            return result;
        }
        catch (Exception ex)
        {
            await _auditService.LogEventAsync("EXTERNAL:TrueConf:Auth", clientIp,
                $"Авторизация TrueConf: clientId={clientId}, ошибка={ex.Message}",
                entityName: "TrueConf");
            throw;
        }
    }

    public Task<TrueConfConference> CreateConferenceAsync(
        CreateTrueConfConferenceRequest request,
        CancellationToken cancellationToken = default)
        => _inner.CreateConferenceAsync(request, cancellationToken);

    public Task<TrueConfConference?> GetConferenceAsync(
        string conferenceId,
        CancellationToken cancellationToken = default)
        => _inner.GetConferenceAsync(conferenceId, cancellationToken);

    public Task<bool> DeleteConferenceAsync(
        string conferenceId,
        CancellationToken cancellationToken = default)
        => _inner.DeleteConferenceAsync(conferenceId, cancellationToken);

    public Task<IReadOnlyList<TrueConfConference>> GetStoppedConferencesAsync(
        string? tag = null,
        CancellationToken cancellationToken = default)
        => _inner.GetStoppedConferencesAsync(tag, cancellationToken);

    public Task<IReadOnlyList<TrueConfUser>> GetUsersAsync(
        CancellationToken cancellationToken = default)
        => _inner.GetUsersAsync(cancellationToken);

    public Task<TrueConfServerInfo?> GetServerInfoAsync(
        CancellationToken cancellationToken = default)
        => _inner.GetServerInfoAsync(cancellationToken);
}
