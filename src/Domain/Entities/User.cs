namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Пользователь системы (users).
/// Учётная запись. Физическое лицо ссылается на пользователя через user_id.
/// </summary>
public class User
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Логин (login). Уникальный идентификатор пользователя.</summary>
    public string Login { get; set; } = string.Empty;

    /// <summary>Фамилия (last_name).</summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>Имя (first_name).</summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Отчество (middle_name).</summary>
    public string? MiddleName { get; set; }

    /// <summary>Email (email).</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Телефон (phone).</summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>Признак внешнего директора (is_external).</summary>
    public bool IsExternal { get; set; }

    /// <summary>Дата создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Идентификатор создателя записи (created_by).</summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>Активна ли учётная запись (is_active).</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Дата окончания действия учётной записи (account_expires_at).</summary>
    public DateTime? AccountExpiresAt { get; set; }

    /// <summary>Дата создания учётной записи в LDAP-каталоге (ldap_created_at).</summary>
    public DateTime? LdapCreatedAt { get; set; }

    /// <summary>Идентификатор мастер-записи MPI (mpi_master_id). Источник: LDAP/AD.</summary>
    public Guid? MpiMasterId { get; set; }

    /// <summary>Признак системного пользователя (is_system).</summary>
    public bool IsSystem { get; set; }

    /// <summary>Токен приглашения (invitation_token).</summary>
    public string? InvitationToken { get; set; }

    /// <summary>Срок действия приглашения (invitation_expires_at).</summary>
    public DateTime? InvitationExpiresAt { get; set; }

    /// <summary>Роли пользователя.</summary>
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    /// <summary>Участие в комитетах.</summary>
    public ICollection<CommitteeMember> CommitteeMembers { get; set; } = new List<CommitteeMember>();

    /// <summary>Бюллетени для голосования.</summary>
    public ICollection<Bulletin> Bulletins { get; set; } = new List<Bulletin>();
}
