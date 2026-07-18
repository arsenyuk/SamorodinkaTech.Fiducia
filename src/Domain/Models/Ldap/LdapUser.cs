namespace SamorodinkaTech.Fiducia.Domain.Models.Ldap;

/// <summary>
/// Пользователь LDAP/AD-каталога.
/// </summary>
public class LdapUser
{
    /// <summary>Уникальное имя (distinguishedName).</summary>
    public string DistinguishedName { get; init; } = string.Empty;

    /// <summary>Логин (sAMAccountName / uid).</summary>
    public string LoginName { get; init; } = string.Empty;

    /// <summary>Отображаемое имя (displayName / cn).</summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>Email (mail).</summary>
    public string? Email { get; init; }

    /// <summary>Должность (title).</summary>
    public string? Title { get; init; }

    /// <summary>Отдел (department).</summary>
    public string? Department { get; init; }

    /// <summary>Телефон (telephoneNumber).</summary>
    public string? Phone { get; init; }

    /// <summary>Член групп (memberOf — DN групп).</summary>
    public IReadOnlyList<string> MemberOf { get; init; } = Array.Empty<string>();
}
