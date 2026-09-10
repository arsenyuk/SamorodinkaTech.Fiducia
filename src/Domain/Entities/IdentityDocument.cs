namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Документ, удостоверяющий личность участника (identity_documents).
/// Хранит данные ДУЛ (паспорт, загранпаспорт и т.д.) в связии один-ко-многим с board_participant.
/// </summary>
public class IdentityDocument
{
    /// <summary>Первичный ключ (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Идентификатор участника (participant_id).</summary>
    public Guid ParticipantId { get; set; }

    /// <summary>Участник.</summary>
    public BoardParticipant? Participant { get; set; }

    /// <summary>Идентификатор вида документа (dul_type_id).</summary>
    public Guid DulTypeId { get; set; }

    /// <summary>Вид документа.</summary>
    public RefDulType? DulType { get; set; }

    /// <summary>Серия документа (series).</summary>
    public string? Series { get; set; }

    /// <summary>Номер документа (number).</summary>
    public string? Number { get; set; }

    /// <summary>Кем выдан (issued_by).</summary>
    public string? IssuedBy { get; set; }

    /// <summary>Дата выдачи (issue_date).</summary>
    public DateOnly? IssueDate { get; set; }

    /// <summary>Код подразделения (department_code).</summary>
    public string? DepartmentCode { get; set; }

    /// <summary>Адрес регистрации (registration_address).</summary>
    public string? RegistrationAddress { get; set; }

    /// <summary>Признак активного документа (is_active). Активным считается документ, предоставленный последним.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Дата создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Дата последнего обновления (updated_at).</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Идентификатор создателя записи (created_by).</summary>
    public Guid? CreatedBy { get; set; }
}
