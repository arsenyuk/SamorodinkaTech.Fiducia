namespace SamorodinkaTech.Fiducia.Domain.Entities;

/// <summary>
/// Участник общества — реестр (board_participant).
/// Хранит актуальный состав участников.
/// Данные ФЛ/ИП — в таблице person. Данные ДУЛ — в таблице identity_documents.
/// </summary>
public class BoardParticipant
{
    /// <summary>Первичный ключ (id).</summary>
    public Guid Id { get; set; }

    /// <summary>Идентификатор юридического лица (legal_entity_id).</summary>
    public Guid LegalEntityId { get; set; }

    /// <summary>Идентификатор участника экосистемы (ecosystem_participant_id).</summary>
    public Guid? EcosystemParticipantId { get; set; }

    /// <summary>Участник экосистемы.</summary>
    public EcosystemParticipant? EcosystemParticipant { get; set; }

    /// <summary>Тип участника: FL — физлицо, UL — юрлицо, IP — ИП (participant_type).</summary>
    public string ParticipantType { get; set; } = "FL";

    /// <summary>Идентификатор физического лица (person_id). Nullable для ЮЛ.</summary>
    public Guid? PersonId { get; set; }

    /// <summary>Физическое лицо.</summary>
    public Person? Person { get; set; }

    // ── ЮЛ ────────────────────────────────────────────────────────

    /// <summary>Наименование юридического лица (company_name).</summary>
    public string? CompanyName { get; set; }

    /// <summary>ИНН юридического лица (company_inn).</summary>
    public string? CompanyInn { get; set; }

    /// <summary>ОГРН юридического лица (company_ogrn).</summary>
    public string? CompanyOgrn { get; set; }

    /// <summary>КПП юридического лица (company_kpp).</summary>
    public string? CompanyKpp { get; set; }

    /// <summary>Адрес юридического лица (company_address).</summary>
    public string? CompanyAddress { get; set; }

    // ── Статус и даты ─────────────────────────────────────────────

    /// <summary>Дата вхождения в состав участников (entry_date).</summary>
    public DateOnly? EntryDate { get; set; }

    /// <summary>Дата выхода из состава (exit_date).</summary>
    public DateOnly? ExitDate { get; set; }

    /// <summary>Действующий участник (is_active).</summary>
    public bool IsActive { get; set; } = true;

    // ── Мета ──────────────────────────────────────────────────────

    /// <summary>Время создания записи (created_at).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Время последнего обновления (updated_at).</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Идентификатор создателя записи (created_by).</summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>Генеральный директор ООО (is_general_director).</summary>
    public bool IsGeneralDirector { get; set; }

    /// <summary>Сведения об ЮЛ (версии).</summary>
    public ICollection<BoardParticipantCompany> Companies { get; set; } = new List<BoardParticipantCompany>();

    /// <summary>Доли участника (версии SCD Type 2).</summary>
    public ICollection<BoardParticipantShare> Shares { get; set; } = new List<BoardParticipantShare>();
}
