using System.DirectoryServices.Protocols;
using System.Net;
using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models.Ldap;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Реализация сервиса LDAP-каталога через System.DirectoryServices.Protocols.
/// Поддерживает Active Directory и OpenLDAP через LDAP-протокол.
/// </summary>
public class LdapService : ILdapService
{
    private readonly string _server;
    private readonly int _port;
    private readonly string _baseDn;
    private readonly string? _bindUser;
    private readonly string? _bindPassword;
    private readonly ILogger<LdapService> _logger;

    /// <summary>
    /// Создаёт экземпляр сервиса LDAP.
    /// </summary>
    /// <param name="server">Адрес LDAP-сервера (например, ldap://dc.company.local).</param>
    /// <param name="port">Порт (389 — LDAP, 636 — LDAPS).</param>
    /// <param name="baseDn">Базовый DN для поиска (например, dc=company,dc=local).</param>
    /// <param name="bindUser">DN пользователя для bind (если нужен).</param>
    /// <param name="bindPassword">Пароль пользователя для bind.</param>
    /// <param name="logger">Логгер.</param>
    public LdapService(
        string server,
        int port,
        string baseDn,
        string? bindUser,
        string? bindPassword,
        ILogger<LdapService> logger)
    {
        _server = server ?? throw new ArgumentNullException(nameof(server));
        _port = port;
        _baseDn = baseDn ?? throw new ArgumentNullException(nameof(baseDn));
        _bindUser = bindUser;
        _bindPassword = bindPassword;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<bool> AuthenticateAsync(
        string login,
        string password,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("LDAP-аутентификация пользователя {Login}", login);

        try
        {
            // Сначала ищем DN пользователя по uid (OpenLDAP) или sAMAccountName (AD)
            var user = await ResolveUserDnAsync(login, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("LDAP-аутентификация: пользователь {Login} не найден", login);
                return false;
            }

            // Bind с полным DN
            var identifier = new LdapDirectoryIdentifier(_server, _port);

            using var connection = new LdapConnection(identifier)
            {
                Credential = new NetworkCredential(user, password),
                AuthType = AuthType.Basic
            };

            connection.SessionOptions.ProtocolVersion = 3;
            connection.Bind();

            _logger.LogDebug("LDAP-аутентификация успешна: {Login}", login);
            return true;
        }
        catch (LdapException ex)
        {
            _logger.LogWarning(ex, "LDAP-аутентификация отклонена: {Login}", login);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<LdapUser?> FindUserByLoginAsync(
        string login,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Поиск пользователя LDAP по логину: {Login}", login);

        // Пробуем uid (OpenLDAP), затем sAMAccountName (Active Directory)
        var filter = $"(|(uid={EscapeLdapFilter(login)})(sAMAccountName={EscapeLdapFilter(login)}))";
        var results = await SearchAsync(filter, 1, cancellationToken);

        var user = results.FirstOrDefault();
        if (user == null)
            _logger.LogDebug("Пользователь LDAP не найден: {Login}", login);

        return user;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<LdapUser>> SearchUsersAsync(
        string filter,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Поиск пользователей LDAP: {Filter}", filter);

        return await SearchAsync(filter, 0, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<LdapUser>> GetGroupMembersAsync(
        string groupDn,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Получение членов группы LDAP: {GroupDn}", groupDn);

        // Читаем объект группы и получаем список member-DN
        var memberDns = await GetGroupMemberDnsAsync(groupDn, cancellationToken);

        if (memberDns.Count == 0)
            return Array.Empty<LdapUser>();

        // Для каждого DN участника ищем пользователя
        var users = new List<LdapUser>(memberDns.Count);
        foreach (var dn in memberDns)
        {
            var user = await ResolveUserByDnAsync(dn, cancellationToken);
            if (user != null)
                users.Add(user);
        }

        _logger.LogInformation("Из группы LDAP получено {Count} пользователей", users.Count);
        return users;
    }

    private async Task<IReadOnlyList<string>> GetGroupMemberDnsAsync(
        string groupDn,
        CancellationToken ct)
    {
        try
        {
            var identifier = new LdapDirectoryIdentifier(_server, _port);
            using var connection = new LdapConnection(identifier)
            {
                AuthType = AuthType.Basic
            };
            connection.SessionOptions.ProtocolVersion = 3;

            if (!string.IsNullOrEmpty(_bindUser))
                connection.Credential = new NetworkCredential(_bindUser, _bindPassword);

            connection.Bind();

            // Ищем саму группу по её DN и запрашиваем атрибут member
            var request = new SearchRequest(
                groupDn,
                "(objectClass=*)",
                SearchScope.Base,
                "member")
            {
                SizeLimit = 1
            };

            var response = (SearchResponse)await Task.Run(
                () => connection.SendRequest(request), ct);

            if (response.Entries.Count == 0)
                return Array.Empty<string>();

            var entry = response.Entries[0];
            return GetMultiStringAttr(entry, "member");
        }
        catch (LdapException ex)
        {
            _logger.LogWarning(ex, "Ошибка чтения группы LDAP: {GroupDn}", groupDn);
            return Array.Empty<string>();
        }
    }

    private async Task<LdapUser?> ResolveUserByDnAsync(
        string userDn,
        CancellationToken ct)
    {
        try
        {
            var identifier = new LdapDirectoryIdentifier(_server, _port);
            using var connection = new LdapConnection(identifier)
            {
                AuthType = AuthType.Basic
            };
            connection.SessionOptions.ProtocolVersion = 3;

            if (!string.IsNullOrEmpty(_bindUser))
                connection.Credential = new NetworkCredential(_bindUser, _bindPassword);

            connection.Bind();

            var request = new SearchRequest(
                userDn,
                "(objectClass=*)",
                SearchScope.Base,
                "distinguishedName",
                "sAMAccountName",
                "uid",
                "displayName",
                "cn",
                "mail",
                "title",
                "department",
                "telephoneNumber")
            {
                SizeLimit = 1
            };

            var response = (SearchResponse)await Task.Run(
                () => connection.SendRequest(request), ct);

            return response.Entries.Count > 0
                ? MapToUser(response.Entries[0])
                : null;
        }
        catch (LdapException ex)
        {
            _logger.LogWarning(ex, "Ошибка чтения пользователя LDAP: {UserDn}", userDn);
            return null;
        }
    }

    private async Task<IReadOnlyList<LdapUser>> SearchAsync(
        string filter,
        int sizeLimit,
        CancellationToken ct)
    {
        try
        {
            var identifier = new LdapDirectoryIdentifier(_server, _port);

            using var connection = new LdapConnection(identifier)
            {
                AuthType = AuthType.Basic
            };

            connection.SessionOptions.ProtocolVersion = 3;

            if (!string.IsNullOrEmpty(_bindUser))
                connection.Credential = new NetworkCredential(_bindUser, _bindPassword);

            connection.Bind();

            var request = new SearchRequest(
                _baseDn,
                filter,
                SearchScope.Subtree,
                "distinguishedName",
                "sAMAccountName",
                "displayName",
                "cn",
                "mail",
                "title",
                "department",
                "telephoneNumber",
                "memberOf")
            {
                SizeLimit = sizeLimit
            };

            var response = (SearchResponse)await Task.Run(
                () => connection.SendRequest(request), ct);

            var users = new List<LdapUser>();

            foreach (SearchResultEntry entry in response.Entries)
            {
                users.Add(MapToUser(entry));
            }

            return users;
        }
        catch (LdapException ex)
        {
            _logger.LogWarning(ex, "Ошибка поиска LDAP: {Filter}", filter);
            return Array.Empty<LdapUser>();
        }
    }

    private async Task<string?> ResolveUserDnAsync(
        string login, CancellationToken ct)
    {
        try
        {
            var identifier = new LdapDirectoryIdentifier(_server, _port);

            using var connection = new LdapConnection(identifier)
            {
                AuthType = AuthType.Basic
            };

            connection.SessionOptions.ProtocolVersion = 3;

            if (!string.IsNullOrEmpty(_bindUser))
                connection.Credential = new NetworkCredential(_bindUser, _bindPassword);

            connection.Bind();

            var filter = $"(|(uid={EscapeLdapFilter(login)})(sAMAccountName={EscapeLdapFilter(login)}))";

            var request = new SearchRequest(
                _baseDn, filter, SearchScope.Subtree, "1.1")
            {
                SizeLimit = 1
            };

            var response = (SearchResponse)await Task.Run(
                () => connection.SendRequest(request), ct);

            return response.Entries.Count > 0
                ? response.Entries[0].DistinguishedName
                : null;
        }
        catch (LdapException ex)
        {
            _logger.LogWarning(ex, "Ошибка поиска DN для {Login}", login);
            return null;
        }
    }

    private static LdapUser MapToUser(SearchResultEntry entry)
    {
        return new LdapUser
        {
            DistinguishedName = GetStringAttr(entry, "distinguishedName"),
            LoginName = GetStringAttr(entry, "uid")
                         ?? GetStringAttr(entry, "sAMAccountName")
                         ?? string.Empty,
            DisplayName = GetStringAttr(entry, "displayName")
                          ?? GetStringAttr(entry, "cn")
                          ?? string.Empty,
            Email = GetStringAttr(entry, "mail"),
            Title = GetStringAttr(entry, "title"),
            Department = GetStringAttr(entry, "department"),
            Phone = GetStringAttr(entry, "telephoneNumber"),
            MemberOf = GetMultiStringAttr(entry, "memberOf"),
            IsActive = ParseUserAccountControl(entry),
            AccountExpiresAt = ParseAccountExpires(entry),
            LdapCreatedAt = ParseCreatedAt(entry)
        };
    }

    /// <summary>
    /// Извлекает дату создания учётной записи из whenCreated (AD) или createTimestamp (OpenLDAP).
    /// Формат: GeneralizedTime (YYYYMMDDHHmmss.0Z).
    /// </summary>
    private static DateTime? ParseCreatedAt(SearchResultEntry entry)
    {
        var raw = GetStringAttr(entry, "whenCreated")
               ?? GetStringAttr(entry, "createTimestamp");
        if (raw == null) return null;

        // GeneralizedTime: "20250101000000.0Z"
        var digits = new string(raw.Where(char.IsDigit).ToArray());
        if (digits.Length < 14) return null;

        try
        {
            return new DateTime(
                int.Parse(digits[..4]),
                int.Parse(digits[4..6]),
                int.Parse(digits[6..8]),
                int.Parse(digits[8..10]),
                int.Parse(digits[10..12]),
                int.Parse(digits[12..14]),
                DateTimeKind.Utc);
        }
        catch
        {
            return null;
        }
    }

    private static string? GetStringAttr(SearchResultEntry entry, string name)
    {
        return entry.Attributes.Contains(name)
            ? entry.Attributes[name][0] as string
            : null;
    }

    private static IReadOnlyList<string> GetMultiStringAttr(
        SearchResultEntry entry, string name)
    {
        if (!entry.Attributes.Contains(name))
            return Array.Empty<string>();

        return entry.Attributes[name].GetValues(typeof(string))
            .Cast<string>()
            .ToList();
    }

    /// <summary>
    /// Извлекает флаг активности из userAccountControl (AD).
    /// UF_ACCOUNTDISABLE = 0x0002. Если атрибут отсутствует — считаем активным.
    /// </summary>
    private static bool ParseUserAccountControl(SearchResultEntry entry)
    {
        if (!entry.Attributes.Contains("userAccountControl"))
            return true;

        var raw = entry.Attributes["userAccountControl"][0]?.ToString();
        if (raw == null || !int.TryParse(raw, out var uac))
            return true;

        return (uac & 0x0002) == 0;
    }

    /// <summary>
    /// Извлекает дату истечения учётной записи из accountExpires (AD FileTime).
    /// 0 или 0x7FFFFFFFFFFFFFFF = никогда не истекает.
    /// </summary>
    private static DateTime? ParseAccountExpires(SearchResultEntry entry)
    {
        if (!entry.Attributes.Contains("accountExpires"))
            return null;

        var raw = entry.Attributes["accountExpires"][0]?.ToString();
        if (raw == null || !long.TryParse(raw, out var fileTime))
            return null;

        if (fileTime == 0 || fileTime == 0x7FFFFFFFFFFFFFFF)
            return null;

        try
        {
            return DateTime.FromFileTimeUtc(fileTime);
        }
        catch
        {
            return null;
        }
    }

    private static string EscapeLdapFilter(string value)
    {
        return value
            .Replace("\\", "\\5c")
            .Replace("*", "\\2a")
            .Replace("(", "\\28")
            .Replace(")", "\\29")
            .Replace("\0", "\\00");
    }
}
