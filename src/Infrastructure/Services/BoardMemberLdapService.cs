using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Domain.Interfaces;
using SamorodinkaTech.Fiducia.Domain.Models.Ldap;

namespace SamorodinkaTech.Fiducia.Infrastructure.Services;

/// <summary>
/// Сервис синхронизации состава Совета директоров из LDAP-каталога.
/// Читает членов группы «Совет директоров» и предлагает маппинг
/// должностей LDAP на типы директоров ref_board_member_types.
/// Контролирует уникальность учётных записей.
/// </summary>
public class BoardMemberLdapService : IBoardMemberLdapService
{
    private readonly ILdapService _ldap;
    private readonly ILogger<BoardMemberLdapService> _logger;
    private readonly string _boardGroupDn;

    /// <summary>
    /// Маппинг должностей LDAP (title) на коды справочника ref_board_member_types.
    /// </summary>
    private static readonly Dictionary<string, string> TitleToMemberType = new(StringComparer.OrdinalIgnoreCase)
    {
        ["председатель совета директоров"] = "EXECUTIVE",
        ["председатель"] = "EXECUTIVE",
        ["chairman"] = "EXECUTIVE",
        ["исполнительный директор"] = "EXECUTIVE",
        ["executive director"] = "EXECUTIVE",
        ["неисполнительный директор"] = "NON_EXECUTIVE",
        ["внешний директор"] = "NON_EXECUTIVE",
        ["non-executive director"] = "NON_EXECUTIVE",
        ["независимый директор"] = "INDEPENDENT",
        ["independent director"] = "INDEPENDENT",
        ["член совета директоров"] = "STAFF",
        ["штатный сотрудник"] = "STAFF",
        ["корпоративный секретарь"] = "STAFF",
        ["секретарь сд"] = "STAFF"
    };

    /// <summary>
    /// Создаёт экземпляр сервиса.
    /// </summary>
    /// <param name="ldap">Сервис LDAP-каталога.</param>
    /// <param name="logger">Логгер.</param>
    /// <param name="boardGroupDn">DN группы «Совет директоров» в LDAP.</param>
    public BoardMemberLdapService(
        ILdapService ldap,
        ILogger<BoardMemberLdapService> logger,
        string boardGroupDn)
    {
        _ldap = ldap ?? throw new ArgumentNullException(nameof(ldap));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _boardGroupDn = boardGroupDn ?? throw new ArgumentNullException(nameof(boardGroupDn));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<BoardMemberCandidate>> GetCandidatesAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Получение кандидатов в СД из LDAP группы {GroupDn}", _boardGroupDn);

        var members = await _ldap.GetGroupMembersAsync(_boardGroupDn, cancellationToken);

        var candidates = new List<BoardMemberCandidate>(members.Count);

        foreach (var user in members)
        {
            candidates.Add(MapToCandidate(user));
        }

        _logger.LogInformation(
            "Из LDAP получено {Count} кандидатов в СД", candidates.Count);

        return candidates;
    }

    /// <inheritdoc />
    public async Task<BoardMemberCandidate?> GetCandidateByLoginAsync(
        string login,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Поиск кандидата в СД по логину: {Login}", login);

        var user = await _ldap.FindUserByLoginAsync(login, cancellationToken);

        if (user == null)
        {
            _logger.LogDebug("Кандидат не найден в LDAP: {Login}", login);
            return null;
        }

        return MapToCandidate(user);
    }

    /// <inheritdoc />
    public bool IsDuplicate(IReadOnlySet<string> existingLogins, string newLogin)
    {
        return existingLogins.Contains(newLogin);
    }

    /// <inheritdoc />
    public IReadOnlyList<string> FindDuplicates(IReadOnlyList<string> logins)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var duplicates = new List<string>();

        foreach (var login in logins)
        {
            if (!seen.Add(login))
                duplicates.Add(login);
        }

        return duplicates;
    }

    private BoardMemberCandidate MapToCandidate(LdapUser user)
    {
        var candidate = new BoardMemberCandidate
        {
            Login = user.LoginName,
            FullName = user.DisplayName,
            Email = user.Email,
            Title = user.Title,
            Phone = user.Phone,
            DistinguishedName = user.DistinguishedName,
            SuggestedMemberTypeCode = MapTitleToMemberType(user.Title)
        };

        return candidate;
    }

    private static string? MapTitleToMemberType(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return null;

        foreach (var (pattern, memberType) in TitleToMemberType)
        {
            if (title.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                return memberType;
        }

        return null;
    }
}
