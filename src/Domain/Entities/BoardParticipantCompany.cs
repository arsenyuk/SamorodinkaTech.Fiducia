namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Сведения об ЮЛ участника (board_participant_company).
/// SCD Type 2: каждая версия — отдельная запись. Активная: is_active = true.
/// Только для UL-участников (participant_type = 'UL').
/// </summary>
public class BoardParticipantCompany
{
    /// <summary>Первичный ключ (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Идентификатор участника (participant_id).</summary>
    public Guid ParticipantId { get; set; }

    /// <summary>Участник общества.</summary>
    public BoardParticipant? Participant { get; set; }

    /// <summary>Наименование ЮЛ (company_name).</summary>
    public string? CompanyName { get; set; }

    /// <summary>ИНН ЮЛ (company_inn).</summary>
    public string? CompanyInn { get; set; }

    /// <summary>ОГРН ЮЛ (company_ogrn).</summary>
    public string? CompanyOgrn { get; set; }

    /// <summary>КПП ЮЛ (company_kpp).</summary>
    public string? CompanyKpp { get; set; }

    /// <summary>Адрес ЮЛ (company_address).</summary>
    public string? CompanyAddress { get; set; }

    /// <summary>Активная версия (is_active).</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Дата создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Дата обновления записи (updated_at).</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Идентификатор создателя записи (created_by).</summary>
    public Guid? CreatedBy { get; set; }
}
