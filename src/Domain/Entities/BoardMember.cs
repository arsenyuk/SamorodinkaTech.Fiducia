namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Член Совета директоров (board_members).
/// Состав утверждается в рамках ОСА.
/// </summary>
public class BoardMember
{
    /// <summary>Идентификатор (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Ссылка на ОСА (osa_meeting_id).</summary>
    public Guid OsaMeetingId { get; set; }

    /// <summary>ФИО члена СД (full_name).</summary>
    public string FullName { get; set; } = default!;

    /// <summary>Тип участника СД (board_member_type_id).</summary>
    public Guid? BoardMemberTypeId { get; set; }

    /// <summary>Учётная запись (account).</summary>
    public string? Account { get; set; }

    /// <summary>Email (email).</summary>
    public string? Email { get; set; }

    /// <summary>Ссылка на пользователя системы (user_id). Заполняется при выборе из LDAP.</summary>
    public Guid? UserId { get; set; }

    /// <summary>Ссылка на Совет директоров (board_of_directors_id).</summary>
    public Guid? BoardOfDirectorsId { get; set; }

    public OsaMeeting? OsaMeeting { get; set; }
    public BoardOfDirectors? BoardOfDirectors { get; set; }
    public BoardMemberType? BoardMemberType { get; set; }
    public User? User { get; set; }
}
