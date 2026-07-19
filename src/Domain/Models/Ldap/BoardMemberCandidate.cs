namespace SamorodinkaTech.Fiducia.Domain.Models.Ldap;

/// <summary>
/// Кандидат в состав Совета директоров, полученный из LDAP-каталога.
/// Содержит данные пользователя и предложенный маппинг на тип директора.
/// </summary>
public class BoardMemberCandidate
{
    /// <summary>LDAP-логин (uid / sAMAccountName).</summary>
    public string Login { get; init; } = string.Empty;

    /// <summary>ФИО (displayName / cn).</summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>Email (mail).</summary>
    public string? Email { get; init; }

    /// <summary>Должность по данным LDAP (title).</summary>
    public string? Title { get; init; }

    /// <summary>Телефон (telephoneNumber).</summary>
    public string? Phone { get; init; }

    /// <summary>DN записи в каталоге (distinguishedName).</summary>
    public string DistinguishedName { get; init; } = string.Empty;

    /// <summary>Предложенный тип директора (код ref_board_member_types).</summary>
    public string? SuggestedMemberTypeCode { get; init; }

    /// <summary>Активна ли учётная запись в LDAP.</summary>
    public bool IsActive { get; init; } = true;

    /// <summary>Дата окончания действия учётной записи в LDAP.</summary>
    public DateTime? AccountExpiresAt { get; init; }

    /// <summary>Дата создания учётной записи в LDAP-каталоге.</summary>
    public DateTime? LdapCreatedAt { get; init; }
}
