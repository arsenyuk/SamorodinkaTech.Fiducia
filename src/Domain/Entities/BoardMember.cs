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

    /// <summary>Должность в организации (position).</summary>
    public string? Position { get; set; }

    /// <summary>Тип участника СД (board_member_type_id).</summary>
    public Guid? BoardMemberTypeId { get; set; }

    public OsaMeeting? OsaMeeting { get; set; }
    public BoardMemberType? BoardMemberType { get; set; }
}
