using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models.Ldap;

namespace SamorodinkaTech.Fiducia.Infrastructure.Auditing;

/// <summary>
/// Декоратор для ILdapService — логирует авторизацию (AuthenticateAsync).
/// </summary>
public class AuditLdapDecorator : ILdapService
{
    private readonly ILdapService _inner;
    private readonly ISecurityAuditService _auditService;
    private readonly IClientIpProvider _ipProvider;
    private readonly ILogger<AuditLdapDecorator> _logger;

    public AuditLdapDecorator(
        ILdapService inner,
        ISecurityAuditService auditService,
        IClientIpProvider ipProvider,
        ILogger<AuditLdapDecorator> logger)
    {
        _inner = inner;
        _auditService = auditService;
        _ipProvider = ipProvider;
        _logger = logger;
    }

    public async Task<bool> AuthenticateAsync(
        string login,
        string password,
        CancellationToken cancellationToken = default)
    {
        var clientIp = _ipProvider.GetClientIp();
        try
        {
            var result = await _inner.AuthenticateAsync(login, password, cancellationToken);
            await _auditService.LogEventAsync("AUTH:LDAP", clientIp,
                $"LDAP-аутентификация: login={login}, результат={(result ? "успех" : "отказ")}",
                entityName: "LDAP");
            return result;
        }
        catch (Exception ex)
        {
            await _auditService.LogEventAsync("AUTH:LDAP", clientIp,
                $"LDAP-аутентификация: login={login}, ошибка={ex.Message}",
                entityName: "LDAP");
            throw;
        }
    }

    public Task<LdapUser?> FindUserByLoginAsync(
        string login,
        CancellationToken cancellationToken = default)
        => _inner.FindUserByLoginAsync(login, cancellationToken);

    public Task<IReadOnlyList<LdapUser>> SearchUsersAsync(
        string filter,
        CancellationToken cancellationToken = default)
        => _inner.SearchUsersAsync(filter, cancellationToken);

    public Task<IReadOnlyList<LdapUser>> GetGroupMembersAsync(
        string groupDn,
        CancellationToken cancellationToken = default)
        => _inner.GetGroupMembersAsync(groupDn, cancellationToken);
}
