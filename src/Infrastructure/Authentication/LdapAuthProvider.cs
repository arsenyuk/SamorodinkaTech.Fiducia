using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Interfaces;

namespace SamorodinkaTech.Fiducia.Infrastructure.Authentication;

/// <summary>
/// Провайдер аутентификации через LDAP/OpenLDAP-каталог с SSO.
/// Проверяет учётные данные через ILdapService и назначает роли
/// на основе членства в LDAP-группах:
///   cn=SysAdmins → SYS_ADMIN
///   cn=BoardOfDirectors → MEMBER_BOARD
/// </summary>
public class LdapAuthProvider : IAuthProvider
{
    private readonly ILdapService _ldap;
    private readonly ILogger<LdapAuthProvider> _logger;
    private readonly string _sysAdminGroupDn;
    private readonly string _boardGroupDn;

    public string ProviderName => "LDAP";

    /// <summary>
    /// Создаёт провайдер LDAP-аутентификации.
    /// </summary>
    /// <param name="ldap">Сервис LDAP.</param>
    /// <param name="logger">Логгер.</param>
    /// <param name="sysAdminGroupDn">DN группы администраторов системы.</param>
    /// <param name="boardGroupDn">DN группы Совета директоров.</param>
    public LdapAuthProvider(
        ILdapService ldap,
        ILogger<LdapAuthProvider> logger,
        string sysAdminGroupDn,
        string boardGroupDn)
    {
        _ldap = ldap ?? throw new ArgumentNullException(nameof(ldap));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _sysAdminGroupDn = sysAdminGroupDn ?? throw new ArgumentNullException(nameof(sysAdminGroupDn));
        _boardGroupDn = boardGroupDn ?? throw new ArgumentNullException(nameof(boardGroupDn));
    }

    /// <inheritdoc />
    public async Task<AuthResult> AuthenticateAsync(string username, string password)
    {
        _logger.LogDebug("LDAP SSO: попытка входа {Username}", username);

        try
        {
            // 1. Проверка учётных данных через LDAP bind
            var authenticated = await _ldap.AuthenticateAsync(username, password);

            if (!authenticated)
            {
                _logger.LogWarning("LDAP SSO: неверные учётные данные для {Username}", username);
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Неверный логин или пароль"
                };
            }

            // 2. Получаем данные пользователя из LDAP
            var user = await _ldap.FindUserByLoginAsync(username);

            if (user == null)
            {
                _logger.LogWarning("LDAP SSO: пользователь {Username} не найден в каталоге", username);
                return new AuthResult
                {
                    Success = false,
                    ErrorMessage = "Пользователь не найден в каталоге"
                };
            }

            // 3. Определяем роль по членству в группах
            var role = ResolveRole(user.MemberOf);

            _logger.LogInformation(
                "LDAP SSO: вход выполнен {Username} ({DisplayName}), роль={Role}",
                username, user.DisplayName, role);

            return new AuthResult
            {
                Success = true,
                UserName = username,
                Claims = new Dictionary<string, string>
                {
                    ["role"] = role,
                    ["display_name"] = user.DisplayName,
                    ["email"] = user.Email ?? string.Empty,
                    ["auth_source"] = "LDAP",
                    ["dn"] = user.DistinguishedName
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "LDAP SSO: ошибка аутентификации {Username}", username);
            return new AuthResult
            {
                Success = false,
                ErrorMessage = $"Ошибка подключения к LDAP: {UnwrapException(ex)}"
            };
        }
    }

    /// <inheritdoc />
    public Task<List<UserInfo>> GetUsersAsync()
    {
        // LDAP не возвращает список пользователей через IAuthProvider —
        // используется ILdapService для операций с каталогом.
        return Task.FromResult(new List<UserInfo>());
    }

    private string ResolveRole(IReadOnlyList<string> memberOf)
    {
        // Приоритет: администратор > член СД > гость
        foreach (var group in memberOf)
        {
            if (group.Contains("SysAdmins", StringComparison.OrdinalIgnoreCase))
                return "SYS_ADMIN";
        }

        foreach (var group in memberOf)
        {
            if (group.Contains("BoardOfDirectors", StringComparison.OrdinalIgnoreCase))
                return "MEMBER_BOARD";
        }

        return "MEMBER_BOARD";
    }

    private static string UnwrapException(Exception ex)
    {
        var inner = ex;
        while (inner.InnerException != null)
            inner = inner.InnerException;

        return inner.Message;
    }
}
