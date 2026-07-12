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
    private readonly ILogger<AuditLdapDecorator> _logger;

    public AuditLdapDecorator(
        ILdapService inner,
        ISecurityAuditService auditService,
        ILogger<AuditLdapDecorator> logger)
    {
        _inner = inner;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<bool> AuthenticateAsync(
        string login,
        string password,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _inner.AuthenticateAsync(login, password, cancellationToken);
            await _auditService.LogEventAsync("EXTERNAL_AUTH:LDAP", "internal",
                $"LDAP-аутентификация: login={login}, результат={(result ? "успех" : "отказ")}",
                entityName: "LDAP");
            return result;
        }
        catch (Exception ex)
        {
            await _auditService.LogEventAsync("EXTERNAL_AUTH:LDAP", "internal",
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
